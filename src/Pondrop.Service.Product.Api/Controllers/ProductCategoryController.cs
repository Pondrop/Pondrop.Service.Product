using AspNetCore.Proxy;
using AspNetCore.Proxy.Options;
using Azure.Search.Documents.Indexes;
using Azure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pondrop.Service.Product.Api.Models;
using Pondrop.Service.Product.Api.Services;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Queries;

namespace Pondrop.Service.ProductCategory.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ProductCategoryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IRebuildCheckpointQueueService _rebuildCheckpointQueueService;
    private readonly SearchIndexConfiguration _searchIdxConfig;
    private readonly ILogger<ProductCategoryController> _logger;
    private readonly SearchIndexerClient _searchIndexerClient;
    private readonly HttpProxyOptions _searchProxyOptions;

    public ProductCategoryController(
        IMediator mediator,
        IServiceBusService serviceBusService,
        IRebuildCheckpointQueueService rebuildCheckpointQueueService,
        IOptions<SearchIndexConfiguration> searchIdxConfig,
        ILogger<ProductCategoryController> logger)
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
    public async Task<IActionResult> GetAllProductCategories()
    {
        var result = await _mediator.Send(new GetAllProductCategoriesQuery());
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpGet("/ParentCategories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllParentCategories()
    {
        var result = await _mediator.Send(new GetAllParentCategoriesQuery());
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductCategoryById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetProductCategoryByIdQuery() { Id = id });

        if (result.IsSuccess)
        {
            await _mediator.Send(new UpdateCategoryWithProductsViewCommand() { ProductCategoryId = id });
            //await _mediator.Send(new UpdateParentProductCategoryViewCommand() { ProductCategoryId = id });
            await _mediator.Send(new UpdateProductWithCategoriesViewCommand() { ProductCategoryId = id });
            //await _mediator.Send(new UpdateParentCategoryViewCommand() { ProductCategoryId = id });
        }
        
        return result.Match<IActionResult>(
            i => i is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProductCategory([FromBody] CreateProductCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateProductCategoryCheckpointByIdCommand()
                {
                    Id = i!.Id,
                    ProductId = i.ProductId,
                    CategoryId = i.CategoryId,
                });
                return StatusCode(StatusCodes.Status201Created, i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProductCategory([FromBody] UpdateProductCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateProductCategoryCheckpointByIdCommand()
                {
                    Id = i!.Id,
                    ProductId = i.ProductId,
                    CategoryId = i.CategoryId,
                });
                return new OkObjectResult(i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("update/checkpoint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCheckpoint([FromBody] UpdateProductCategoryCheckpointByIdCommand command)
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
        _rebuildCheckpointQueueService.Queue(new RebuildProductCategoryCheckpointCommand());
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
            _searchIdxConfig.ProductCategoryIndexName,
            $"docs?api-version=2021-04-30-Preview&{queryString}");

        return this.HttpProxyAsync(url, _searchProxyOptions);
    }

    [HttpGet]
    [Route("indexer/run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunIndexer()
    {
        var response = await _searchIndexerClient.RunIndexerAsync(_searchIdxConfig.ProductCategoryIndexerName);

        if (response.IsError)
            return new BadRequestObjectResult(response.ReasonPhrase);

        return new AcceptedResult();
    }
}