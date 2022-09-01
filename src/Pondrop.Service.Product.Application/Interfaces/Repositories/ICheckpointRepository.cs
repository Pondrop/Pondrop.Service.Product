using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Interfaces;

public interface ICheckpointRepository<T> : IContainerRepository<T> where T : EventEntity
{
    Task<int> RebuildAsync();
    Task<T?> UpsertAsync(long expectedVersion, T item);

    Task FastForwardAsync(T item);
}