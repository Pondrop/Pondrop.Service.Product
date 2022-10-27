using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Interfaces;

public interface IContainerRepository<T>
{
    Task<bool> IsConnectedAsync();

    Task<T?> UpsertAsync(T item);

    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);

    Task<List<T>> QueryAsync(string sqlQueryText, Dictionary<string, string>? parameters = null);
    Task<List<TEntity>> QueryAsync<TEntity>(string sqlQueryText, Dictionary<string, string>? parameters = null);
}