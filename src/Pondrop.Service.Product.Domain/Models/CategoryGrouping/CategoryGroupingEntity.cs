using Newtonsoft.Json;
using Pondrop.Service.Product.Domain.Events;
using Pondrop.Service.Product.Domain.Events.CategoryGrouping;

namespace Pondrop.Service.Product.Domain.Models;

public record CategoryGroupingEntity : EventEntity
{
    public CategoryGroupingEntity()
    {
        Id = Guid.Empty;
        HigherLevelCategoryId = Guid.Empty;
        LowerLevelCategoryId = Guid.Empty;
        Description = string.Empty;
        PublicationLifecycleId = string.Empty;
    }

    public CategoryGroupingEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public CategoryGroupingEntity(Guid higherLevelCategoryId, Guid lowerLevelCategoryId, string description, string publicationLifecycleId, string createdBy) : this()
    {
        var create = new CreateCategoryGrouping(Guid.NewGuid(), higherLevelCategoryId, lowerLevelCategoryId, description, publicationLifecycleId);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "higherLevelCategoryId")]
    public Guid HigherLevelCategoryId { get; private set; }

    [JsonProperty(PropertyName = "lowerLevelCategoryId")]
    public Guid LowerLevelCategoryId { get; private set; }

    [JsonProperty(PropertyName = "description")]
    public string Description { get; private set; }

    [JsonProperty(PropertyName = "publicationLifecycleID")]
    public string PublicationLifecycleId { get; private set; }


    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateCategoryGrouping create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateCategoryGrouping update:
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
        if (eventPayloadToApply is CreateCategoryGrouping create)
        {
            Apply(new Event(GetStreamId<CategoryGroupingEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateCategoryGrouping create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        HigherLevelCategoryId = create.HigherLevelCategoryId;
        LowerLevelCategoryId = create.LowerLevelCategoryId;
        Description = create.Description;
        PublicationLifecycleId = create.PublicationLifecycleId;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateCategoryGrouping update, string createdBy, DateTime createdUtc)
    {
        var oldHigherLevelCategoryId = HigherLevelCategoryId;
        var oldLowerLevelCategoryId = LowerLevelCategoryId;
        var oldDescription = Description;
        var oldPublicationLifecycleID = PublicationLifecycleId;

        HigherLevelCategoryId = update.HigherLevelCategoryId ?? HigherLevelCategoryId;
        LowerLevelCategoryId = update.LowerLevelCategoryId ?? LowerLevelCategoryId;
        Description = update.Description ?? Description;
        PublicationLifecycleId = update.PublicationLifecycleId ?? PublicationLifecycleId;

        if (oldHigherLevelCategoryId != HigherLevelCategoryId ||
            oldLowerLevelCategoryId != LowerLevelCategoryId ||
            oldDescription != Description ||
            oldPublicationLifecycleID != PublicationLifecycleId)
        {
            UpdatedBy = createdBy;
            UpdatedUtc = createdUtc;
        }
    }
}