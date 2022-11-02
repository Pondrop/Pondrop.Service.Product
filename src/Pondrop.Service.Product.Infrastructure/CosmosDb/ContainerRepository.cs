using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Product.Application.Models;

namespace Pondrop.Service.Product.Infrastructure.CosmosDb;

public class ContainerRepository<T> : AbstractRepository<T>
{
    public ContainerRepository(
        IOptions<CosmosConfiguration> config,
        ILogger<ContainerRepository<T>> logger) : base(GetContainerName(), config, logger)
    {
        
    }

    protected override Task OnConnectedAsync() => Task.CompletedTask;

    private static string GetContainerName()
    {
        var nameChars = typeof(T).Name
            .Replace("View", string.Empty)
            .Replace("Entity", string.Empty)
            .Replace("Record", string.Empty)
            .ToCharArray();
        nameChars[0] = char.ToLower(nameChars[0]);
        return $"{new string(nameChars)}_view";
    }
}
