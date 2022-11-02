using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Product.Application.Interfaces;
using Pondrop.Service.Product.Application.Models;
using System.Net;

namespace Pondrop.Service.Product.Infrastructure.CosmosDb;

public abstract class AbstractRepository<T> : IContainerRepository<T>
{
    private readonly SemaphoreSlim _connectSemaphore = new SemaphoreSlim(1, 1);
    
    protected string ContainerName { get; private set; }

    protected ILogger Logger { get; private set; }
    protected CosmosConfiguration Config { get; private set; }

    protected  CosmosClient CosmosClient { get; private set; }

    protected Database? Database { get; private set; }
    protected Container? Container { get; private set; }

    public AbstractRepository(
        string containerName,
        IOptions<CosmosConfiguration> config,
        ILogger logger)
    {
        ContainerName = containerName;
        
        Logger = logger;
        Config = config.Value;

        CosmosClient =
            new CosmosClient(
                Config.ConnectionString,
                new CosmosClientOptions()
                {
                    AllowBulkExecution = true,
                    ApplicationName = Config.ApplicationName,
                    SerializerOptions = new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
                });
    }

    protected abstract Task OnConnectedAsync();

    public async Task<bool> IsConnectedAsync()
    {
        if (Container is not null)
            return true;

        await _connectSemaphore.WaitAsync();

        try
        {
            if (Container is null)
            {
                Database = await CosmosClient.CreateDatabaseIfNotExistsAsync(Config.DatabaseName);
            }

            if (Database is not null && Container is null)
            {
                var containerProperties = new ContainerProperties(ContainerName, "/id");
                var autoscaleThroughputProperties = ThroughputProperties.CreateAutoscaleThroughput(1000);
                Container = await Database.CreateContainerIfNotExistsAsync(containerProperties, autoscaleThroughputProperties);
            }

            await OnConnectedAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
        }
        finally
        {
            _connectSemaphore.Release();
        }

        return Container is not null;
    }

    public async Task<T?> UpsertAsync(T entity)
    {
        if (await IsConnectedAsync())
        {
            var response = await Container!.UpsertItemAsync(entity);
            return response.Resource;
        }

        return default;
    }

    public async Task<List<T>> GetAllAsync()
    {
        var list = new List<T>();
        if (await IsConnectedAsync())
        {
            const string sql = "SELECT * FROM c";
            var iterator = Container!.GetItemQueryIterator<T>(sql);
            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                list.AddRange(page.Resource);
            }
        }
        return list;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        if (id != Guid.Empty && await IsConnectedAsync())
        {
            var idString = id.ToString();
            try
            {
                var result = await Container!.ReadItemAsync<T>(idString, new PartitionKey(idString));
                return result.Resource;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                // eat
            }
        }

        return default;
    }
    
    public async Task<List<T>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        if (ids.Any() && await IsConnectedAsync())
        {
            var idsString = string.Join(", ", ids.Select(i => $"'{i}'"));
            return await QueryAsync<T>($"SELECT * FROM c WHERE c.id IN ({idsString})");
        }

        return new List<T>(0);
    }

    public Task<List<T>> QueryAsync(string sqlQueryText, Dictionary<string, string>? parameters = null)
    {
        return QueryAsync<T>(sqlQueryText, parameters);
    }
    
    public async Task<List<TEntity>> QueryAsync<TEntity>(string sqlQueryText, Dictionary<string, string>? parameters = null)
    {
        var list = new List<TEntity>();

        if (!string.IsNullOrEmpty(sqlQueryText) && await IsConnectedAsync())
        {
            var queryDefinition = new QueryDefinition(sqlQueryText);

            if (parameters?.Any() == true)
            {
                foreach (var kv in parameters)
                {
                    queryDefinition = queryDefinition.WithParameter(kv.Key, kv.Value);
                }
            }

            var iterator = Container!.GetItemQueryIterator<TEntity>(queryDefinition);
            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                list.AddRange(page.Resource);
            }
        }

        return list;
    }
}
