using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
using Pondrop.Service.Product.Domain.Events;
using Pondrop.Service.Product.Domain.Events.Category;

namespace Pondrop.Service.Product.Domain.Models;

public record CategoryEntity : EventEntity
{
    public CategoryEntity()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        Type = string.Empty;
        PublicationLifecycleId = string.Empty;
    }

    public CategoryEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public CategoryEntity(string name, string type, string publicationLifecycleId, string createdBy) : this()
    {
        var create = new CreateCategory(Guid.NewGuid(), name, type, publicationLifecycleId);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; private set; }

    [JsonProperty(PropertyName = "type")]
    public string Type { get; private set; }

    [JsonProperty(PropertyName = "publicationLifecycleID")]
    public string PublicationLifecycleId { get; private set; }


    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateCategory create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateCategory update:
                When(update, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            default:
                throw new InvalidOperationException($"Unrecognised event type for '{StreamType}', got '{eventToApply.GetType().Name}'");
        }

        Events.Add(eventToApply);

        AtSequence = eventToApply.SequenceNumber;
    }

    public sealed override void Apply(IEventPayload eventPayloadToApply, string createdBy)
    {
        if (eventPayloadToApply is CreateCategory create)
        {
            Apply(new Event(GetStreamId<CategoryEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateCategory create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        Name = create.Name;
        Type = create.Type;
        PublicationLifecycleId = create.PublicationLifecycleId;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateCategory update, string createdBy, DateTime createdUtc)
    {
        var oldName = Name;
        var oldType = Type;
        var oldPublicationLifecycleID = PublicationLifecycleId;

        Name = update.Name ?? Name;
        Type = update.Type ?? Type;
        PublicationLifecycleId = update.PublicationLifecycleId ?? PublicationLifecycleId;

        if (oldName != Name ||
            oldType != Type ||
            oldPublicationLifecycleID != PublicationLifecycleId)
        {
            UpdatedBy = createdBy;
            UpdatedUtc = createdUtc;
        }
    }
}