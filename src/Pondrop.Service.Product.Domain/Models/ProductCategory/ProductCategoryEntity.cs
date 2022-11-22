using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
using Pondrop.Service.Product.Domain.Events;
using Pondrop.Service.Product.Domain.Events.Product;
using Pondrop.Service.Product.Domain.Events.ProductCategory;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.ProductCategory.Domain.Models;

public record ProductCategoryEntity : EventEntity
{
    public ProductCategoryEntity()
    {
        Id = Guid.Empty;
        CategoryId = Guid.Empty;
        ProductId = Guid.Empty;
        PublicationLifecycleId = string.Empty;
    }

    public ProductCategoryEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public ProductCategoryEntity(
        Guid categoryId,
        Guid productId,
        string publicationLifecycleId, string createdBy) : this()
    {
        var create = new CreateProductCategory(Guid.NewGuid(), categoryId, productId, publicationLifecycleId);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "categoryId")]
    public Guid CategoryId { get; private set; }

    [JsonProperty(PropertyName = "productId")]
    public Guid ProductId { get; private set; }

    [JsonProperty(PropertyName = "publicationLifecycleID")]
    public string PublicationLifecycleId { get; private set; }

    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateProductCategory create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateProductCategory update:
                When(update, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case DeleteProductCategory delete:
                When(delete, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            default:
                throw new InvalidOperationException($"Unrecognised event type for '{StreamType}', got '{eventToApply.GetType().Name}'");
        }

        Events.Add(eventToApply);

        AtSequence = eventToApply.SequenceNumber;
    }

    public sealed override void Apply(IEventPayload eventPayloadToApply, string createdBy)
    {
        if (eventPayloadToApply is CreateProductCategory create)
        {
            Apply(new Event(GetStreamId<ProductCategoryEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateProductCategory create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        CategoryId = create.CategoryId;
        ProductId = create.ProductId;
        PublicationLifecycleId = create.PublicationLifecycleId;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateProductCategory update, string createdBy, DateTime createdUtc)
    {
        var oldProductId = ProductId;
        var oldCategoryId = CategoryId;
        var oldPublicationLifecycleId = PublicationLifecycleId;

        PublicationLifecycleId = update.PublicationLifecycleId ?? PublicationLifecycleId;

        if (oldProductId != ProductId ||
            oldCategoryId != CategoryId ||
            oldPublicationLifecycleId != PublicationLifecycleId)
        {
            UpdatedBy = createdBy;
            UpdatedUtc = createdUtc;
        }
    }
    private void When(DeleteProductCategory delete, string createdBy, DateTime deletedUtc)
    {
        UpdatedBy = createdBy;
        UpdatedUtc = deletedUtc;
        DeletedUtc = deletedUtc;
    }
}