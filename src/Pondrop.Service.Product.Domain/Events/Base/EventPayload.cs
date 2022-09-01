using Newtonsoft.Json;

namespace Pondrop.Service.Product.Domain.Events;

public record EventPayload : IEventPayload
{
    public DateTime CreatedUtc { get; } = DateTime.UtcNow;
}