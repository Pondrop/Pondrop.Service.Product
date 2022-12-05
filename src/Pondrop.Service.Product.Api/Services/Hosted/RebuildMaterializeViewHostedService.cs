using MediatR;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Commands;

namespace Pondrop.Service.Product.Api.Services;

public class RebuildMaterializeViewHostedService : BackgroundService
{
    private readonly IRebuildCheckpointQueueService _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RebuildMaterializeViewHostedService> _logger;

    public RebuildMaterializeViewHostedService(
        IRebuildCheckpointQueueService queue,
        IServiceProvider serviceProvider,
        ILogger<RebuildMaterializeViewHostedService> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var command = await _queue.DequeueAsync(stoppingToken);

            try
            {
                var mediator = _serviceProvider.GetService<IMediator>();
                //await mediator!.Send(command, stoppingToken);

                switch (command)
                {
                    case RebuildProductCheckpointCommand product:
                        await mediator!.Send(new RebuildProductViewCommand(), stoppingToken);
                        break;
                    case RebuildCategoryCheckpointCommand category:
                        await mediator!.Send(new RebuildCategoryViewCommand(), stoppingToken);
                        await mediator!.Send(new RebuildParentCategoryViewCommand(), stoppingToken);
                        break;
                    case RebuildCategoryGroupingCheckpointCommand categoryGrouping:
                        await mediator!.Send(new RebuildCategoryGroupingViewCommand(), stoppingToken);
                        await mediator!.Send(new RebuildParentCategoryViewCommand(), stoppingToken);
                        break;
                    case RebuildProductCategoryCheckpointCommand productCategory:
                        await mediator!.Send(new RebuildParentCategoryViewCommand(), stoppingToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to run rebuild materialize view {command.GetType().Name}");
            }
        }
    }
}