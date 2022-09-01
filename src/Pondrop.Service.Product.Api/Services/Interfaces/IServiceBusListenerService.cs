using Pondrop.Service.Product.Application.Commands;
using System.Collections.Concurrent;

namespace Pondrop.Service.Product.Api.Services;

public interface IServiceBusListenerService
{
    Task StartListener();

    Task StopListener();

    ValueTask DisposeAsync();
}