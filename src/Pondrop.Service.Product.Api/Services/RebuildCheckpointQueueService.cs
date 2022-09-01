using Pondrop.Service.Product.Application.Commands;

namespace Pondrop.Service.Product.Api.Services;

public class RebuildCheckpointQueueService : BaseBackgroundQueueService<RebuildCheckpointCommand>, IRebuildCheckpointQueueService
{
}