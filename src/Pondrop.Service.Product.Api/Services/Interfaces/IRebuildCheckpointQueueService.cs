using Pondrop.Service.Product.Application.Commands;

namespace Pondrop.Service.Product.Api.Services;

public interface IRebuildCheckpointQueueService
{
    Task<RebuildCheckpointCommand> DequeueAsync(CancellationToken cancellationToken);
    void Queue(RebuildCheckpointCommand command);
}