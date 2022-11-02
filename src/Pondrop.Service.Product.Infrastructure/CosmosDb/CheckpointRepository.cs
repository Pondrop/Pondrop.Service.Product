using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Infrastructure.CosmosDb;

public class CheckpointRepository<T> : AbstractRepository<T>, ICheckpointRepository<T> where T : EventEntity, new()
{
    private const string SpUpsertCheckpoint = "spUpsertCheckpoint";

    private readonly IEventRepository _eventRepository;

    public CheckpointRepository(
        IEventRepository eventRepository,
        IOptions<CosmosConfiguration> config,
        ILogger<CheckpointRepository<T>> logger) : base(GetContainerName(), config, logger)
    {
        _eventRepository = eventRepository;
    }

    protected override async Task OnConnectedAsync()
    {
        if (Container is not null)
        {
            var spDirInfo = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"CosmosDb/StoredProcedures/Checkpoint"));
            await Container.EnsureStoreProcedures(spDirInfo);
        }
    }

    public async Task<int> RebuildAsync()
    {
        if (await IsConnectedAsync())
        {
            var streamType = EventEntity.GetStreamTypeName<T>();

            var allStreams = await _eventRepository.LoadStreamsByTypeAsync(streamType);

            foreach (var i in allStreams)
            {
                var entity = new T();
                entity.Apply(i.Value.Events);
                await Container!.UpsertItemAsync(entity);
            }

            return allStreams.Count;
        }

        return -1;
    }

    public async Task<T?> UpsertAsync(long expectedVersion, T entity)
    {
        if (await IsConnectedAsync())
        {
            var idString = entity.Id.ToString();

            var parameters = new dynamic[]
            {
                idString,
                expectedVersion,
                JsonConvert.SerializeObject(entity)
            };

            return await Container!.Scripts.ExecuteStoredProcedureAsync<T?>(SpUpsertCheckpoint,
                new PartitionKey(idString), parameters);
        }

        return default;
    }

    public async Task FastForwardAsync(T item)
    {
        var eventStream = await _eventRepository.LoadStreamAsync(item.StreamId, item.AtSequence + 1);
        if (eventStream.Events.Any())
            item.Apply(eventStream.Events);
    }

    private static string GetContainerName()
    {
        var nameChars = typeof(T).Name
            .Replace("Entity", string.Empty)
            .Replace("Record", string.Empty)
            .ToCharArray();
        nameChars[0] = char.ToLower(nameChars[0]);
        return $"{new string(nameChars)}_checkpoint";
    }
}
