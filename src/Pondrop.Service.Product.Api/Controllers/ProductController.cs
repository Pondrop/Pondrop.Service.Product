using AspNetCore.Proxy;
using AspNetCore.Proxy.Options;
using Azure;
using Azure.Search.Documents.Indexes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pondrop.Service.Product.Api.Models;
using Pondrop.Service.Product.Api.Services;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Queries;

namespace Pondrop.Service.Product.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IRebuildCheckpointQueueService _rebuildCheckpointQueueService;
    private readonly SearchIndexConfiguration _searchIdxConfig;
    private readonly ILogger<ProductController> _logger;

    private readonly SearchIndexerClient _searchIndexerClient;
    private readonly HttpProxyOptions _searchProxyOptions;

    public ProductController(
        IMediator mediator,
        IServiceBusService serviceBusService,
        IRebuildCheckpointQueueService rebuildCheckpointQueueService,
        IOptions<SearchIndexConfiguration> searchIdxConfig,
        ILogger<ProductController> logger)
    {
        _mediator = mediator;
        _serviceBusService = serviceBusService;
        _rebuildCheckpointQueueService = rebuildCheckpointQueueService;
        _searchIdxConfig = searchIdxConfig.Value;
        _logger = logger;


        _searchIndexerClient = new SearchIndexerClient(new Uri(_searchIdxConfig.BaseUrl), new AzureKeyCredential(_searchIdxConfig.ManagementKey));

        _searchProxyOptions = HttpProxyOptionsBuilder
            .Instance
            .WithBeforeSend((httpContext, requestMessage) =>
            {
                requestMessage.Headers.Add("api-key", _searchIdxConfig.ApiKey);
                return Task.CompletedTask;
            }).Build();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllProducts(int offset = 0, int limit = 10)
    {
        var result = await _mediator.Send(new GetAllProductsQuery()
        {
            Offset = offset,
            Limit = limit
        });
        return result.Match<IActionResult>(
            i => new OkObjectResult(new { Items = i, Offset = offset, Limit = limit, count = i!.Count() }
            ),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery() { Id = id });
        return result.Match<IActionResult>(
            i => i is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateProductCheckpointByIdCommand() { Id = i!.Id });
                return StatusCode(StatusCodes.Status201Created, i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("createfull")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFullProduct([FromBody] CreateProductWithCategoriesCommand command)
    {
        var result = await _mediator.Send(new CreateProductCommand()
        {
            Name = command.Name,
            AltName = command.AltName,
            ShortDescription = command.ShortDescription,
            BrandId = command.BrandId,
            NetContent = command.NetContent,
            NetContentUom = command.NetContentUom,
            ChildProductId = command.ChildProductId,
            Variant = command.Variant,
            PossibleCategories = command.PossibleCategories,
            PublicationLifecycleId = command.PublicationLifecycleId,
            ExternalReferenceId = command.ExternalReferenceId,
        });

        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateProductCheckpointByIdCommand() { Id = i!.Id });
                var productCategoryResult = await _mediator.Send(new SetProductCategoriesCommand() { ProductId = i!.Id, CategoryIds = command!.CategoryIds, PublicationLifecycleId = command.PublicationLifecycleId });
                await productCategoryResult.MatchAsync<IActionResult>(
                    async productCategories =>
                    {
                        if (productCategories != null)
                        {
                            foreach (var productCategory in productCategories)
                            {
                                await _serviceBusService.SendMessageAsync(new UpdateProductCategoryCheckpointByIdCommand() { Id = productCategory!.Id, ProductId = productCategory!.ProductId, CategoryId = productCategory!.CategoryId });
                            }
                        }
                        return StatusCode(StatusCodes.Status201Created, i);
                    }, (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));

                var barcodeResult = await _mediator.Send(new CreateBarcodeCommand() { ProductId = i!.Id, RetailerId = i!.Id, CompanyId = i!.Id, BarcodeNumber = command.BarcodeNumber, BarcodeText = command.BarcodeNumber, BarcodeType = "GTIN", PublicationLifecycleId = i!.PublicationLifecycleId });
                await barcodeResult.MatchAsync<IActionResult>(
                    async barcode =>
                    {
                        await _serviceBusService.SendMessageAsync(new UpdateBarcodeCheckpointByIdCommand() { Id = barcode!.Id, ProductId = barcode!.ProductID });

                        return StatusCode(StatusCodes.Status201Created, i);
                    }, (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));

                return StatusCode(StatusCodes.Status201Created, i);
            }, (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }


    [HttpPost]
    [Route("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateProductCheckpointByIdCommand() { Id = i!.Id });
                return new OkObjectResult(i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("update/checkpoint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCheckpoint([FromBody] UpdateProductCheckpointByIdCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("rebuild/checkpoint")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult RebuildCheckpoint()
    {
        _rebuildCheckpointQueueService.Queue(new RebuildProductCheckpointCommand());
        return new AcceptedResult();
    }

    [HttpGet, HttpPost]
    [Route("search")]
    public Task ProxySearchCatchAll()
    {
        var queryString = this.Request.QueryString.Value?.TrimStart('?') ?? string.Empty;
        var url = Path.Combine(
            _searchIdxConfig.BaseUrl,
            "indexes",
            _searchIdxConfig.ProductIndexName,
            $"docs?api-version=2021-04-30-Preview&{queryString}");

        return this.HttpProxyAsync(url, _searchProxyOptions);
    }

    [HttpGet]
    [Route("indexer/run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunIndexer()
    {
        var response = await _searchIndexerClient.RunIndexerAsync(_searchIdxConfig.ProductIndexerName);

        if (response.IsError)
            return new BadRequestObjectResult(response.ReasonPhrase);

        return new AcceptedResult();
    }
}