using Newtonsoft.Json;
using Pondrop.Service.Product.Domain.Events;
using Pondrop.Service.Product.Domain.Events.Category;

namespace Pondrop.Service.Product.Domain.Models;

public record CategoryEntity : EventEntity
{
    public CategoryEntity()
    {
        Id = Guid.Empty;
        CategoryName = string.Empty;
        Description = string.Empty;
        PublicationLifecycleId = string.Empty;
    }

    public CategoryEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public CategoryEntity(string categoryName, string description, string publicationLifecycleId, string createdBy) : this()
    {
        var create = new CreateCategory(Guid.NewGuid(), categoryName, description, publicationLifecycleId);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "categoryName")]
    public string CategoryName { get; private set; }

    [JsonProperty(PropertyName = "description")]
    public string Description { get; private set; }

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
        CategoryName = create.CategoryName;
        Description = create.Description;
        PublicationLifecycleId = create.PublicationLifecycleId;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateCategory update, string createdBy, DateTime createdUtc)
    {
        var oldCategoryName = CategoryName;
        var oldDescription = Description;
        var oldPublicationLifecycleID = PublicationLifecycleId;

        CategoryName = update.CategoryName ?? CategoryName;
        Description = update.Description ?? Description;
        PublicationLifecycleId = update.PublicationLifecycleId ?? PublicationLifecycleId;

        if (oldCategoryName != CategoryName ||
            oldDescription != Description ||
            oldPublicationLifecycleID != PublicationLifecycleId)
        {
            UpdatedBy = createdBy;
            UpdatedUtc = createdUtc;
        }
    }
}