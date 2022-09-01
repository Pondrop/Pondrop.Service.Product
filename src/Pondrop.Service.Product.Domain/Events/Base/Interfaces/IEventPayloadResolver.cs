namespace Pondrop.Service.Product.Domain.Events;

public interface IEventTypePayloadResolver
{
    Type? GetEventPayloadType(string streamType, string typeName);
}