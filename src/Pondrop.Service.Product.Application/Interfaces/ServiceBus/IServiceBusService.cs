using Pondrop.Service.Product.Domain.Events;

namespace Pondrop.Service.Product.Application.Interfaces;

public interface IServiceBusService
{
    Task SendMessageAsync(object payload);

    Task SendMessageAsync(string subject, object payload);
}