using AspNetCore.Proxy;
using AspNetCore.Proxy.Options;
using Azure;
using Azure.Search.Documents.Indexes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Product.Api.Models;
using Pondrop.Service.Product.Api.Services;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Queries;

namespace Pondrop.Service.Product.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CategoryGroupingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IRebuildCheckpointQueueService _rebuildCheckpointQueueService;
    private readonly SearchIndexConfiguration _searchIdxConfig;
    private readonly ILogger<CategoryGroupingController> _logger;
    private readonly SearchIndexerClient _searchIndexerClient;
    private readonly HttpProxyOptions _searchProxyOptions;

    public CategoryGroupingController(
        IMediator mediator,
        IServiceBusService serviceBusService,
        IRebuildCheckpointQueueService rebuildCheckpointQueueService,
        IOptions<SearchIndexConfiguration> searchIdxConfig,
        ILogger<CategoryGroupingController> logger)
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
    public async Task<IActionResult> GetAllCategoryGroupings(int offset = 0, int limit = 10)
    {
        var result = await _mediator.Send(new GetAllCategoryGroupingsQuery()
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
    public async Task<IActionResult> GetCategoryGroupingById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetCategoryGroupingByIdQuery() { Id = id });
        return result.Match<IActionResult>(
            i => i is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategoryGrouping([FromBody] CreateCategoryGroupingCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateCategoryGroupingCheckpointByIdCommand() { Id = i!.Id , LowerCategoryId = i!.LowerLevelCategoryId});
                return StatusCode(StatusCodes.Status201Created, i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCategoryGrouping([FromBody] UpdateCategoryGroupingCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateCategoryGroupingCheckpointByIdCommand() { Id = i!.Id });
                return new OkObjectResult(i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("update/checkpoint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCheckpoint([FromBody] UpdateCategoryGroupingCheckpointByIdCommand command)
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
        _rebuildCheckpointQueueService.Queue(new RebuildCategoryGroupingCheckpointCommand());
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
            _searchIdxConfig.CategoryGroupingIndexName,
            $"docs?api-version=2021-04-30-Preview&{queryString}");

        return this.HttpProxyAsync(url, _searchProxyOptions);
    }

    [HttpGet]
    [Route("indexer/run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunIndexer()
    {
        var response = await _searchIndexerClient.RunIndexerAsync(_searchIdxConfig.CategoryGroupingIndexerName);

        if (response.IsError)
            return new BadRequestObjectResult(response.ReasonPhrase);

        return new AcceptedResult();
    }
}