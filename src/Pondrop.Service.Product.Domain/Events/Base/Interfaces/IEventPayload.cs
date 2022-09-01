using Newtonsoft.Json.Linq;

namespace Pondrop.Service.Product.Domain.Events;

public interface IEventPayload
{
    DateTime CreatedUtc { get; }
}