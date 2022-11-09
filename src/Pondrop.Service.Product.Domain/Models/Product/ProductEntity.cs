using Newtonsoft.Json;
using Pondrop.Service.Events;
using Pondrop.Service.Models;
using Pondrop.Service.Product.Domain.Events;
using Pondrop.Service.Product.Domain.Events.Product;

namespace Pondrop.Service.Product.Domain.Models.Product;

public record ProductEntity : EventEntity
{
    public ProductEntity()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        BrandId = Guid.Empty;
        ExternalReferenceId = string.Empty;
        Variant = string.Empty;
        AltName = string.Empty;
        ShortDescription = string.Empty;
        NetContent = 0;
        NetContentUom = string.Empty;
        PossibleCategories = string.Empty;
        ChildProductId = new List<Guid>();
        PublicationLifecycleId = string.Empty;
    }

    public ProductEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public ProductEntity(string name,
        Guid brandId,
        string externalReferenceId,
        string variant,
        string altName,
        string shortDescription,
        double netContent,
        string netContentUom,
        string possibleCategories,
        string publicationLifecycleId,
        List<Guid> childProductId, string createdBy) : this()
    {
        var create = new CreateProduct(Guid.NewGuid(), name, brandId, externalReferenceId, variant, altName, shortDescription, netContent, netContentUom, possibleCategories, publicationLifecycleId, childProductId);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; private set; }

    [JsonProperty(PropertyName = "brandId")]
    public Guid BrandId { get; private set; }

    [JsonProperty(PropertyName = "externalReferenceId")]
    public string ExternalReferenceId { get; private set; }

    [JsonProperty(PropertyName = "variant")]
    public string Variant { get; private set; }

    [JsonProperty(PropertyName = "altName")]
    public string AltName { get; private set; }

    [JsonProperty(PropertyName = "shortDescription")]
    public string ShortDescription { get; private set; }

    [JsonProperty(PropertyName = "netContent")]
    public double NetContent { get; private set; }

    [JsonProperty(PropertyName = "netContentUom")]
    public string NetContentUom { get; private set; }

    [JsonProperty(PropertyName = "possibleCategories")]
    public string PossibleCategories { get; private set; }

    [JsonProperty(PropertyName = "childProductID")]
    public List<Guid> ChildProductId { get; private set; }

    [JsonProperty(PropertyName = "publicationLifecycleID")]
    public string PublicationLifecycleId { get; private set; }



    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateProduct create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateProduct update:
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
        if (eventPayloadToApply is CreateProduct create)
        {
            Apply(new Event(GetStreamId<ProductEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateProduct create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        Name = create.Name;
        BrandId = create.BrandId;
        Variant = create.Variant;
        AltName = create.AltName;
        ShortDescription = create.ShortDescription;
        NetContent = create.NetContent;
        NetContentUom = create.NetContentUom;
        PossibleCategories = create.PossibleCategories;
        ChildProductId = create.ChildProductId;
        ExternalReferenceId = create.ExternalReferenceId;
        PublicationLifecycleId = create.PublicationLifecycleId;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateProduct update, string createdBy, DateTime createdUtc)
    {
        var oldName = Name;
        var oldBrandId = BrandId;
        var oldExternalReferenceId = ExternalReferenceId;
        var oldVariant = Variant;
        var oldAltName = AltName;
        var oldShortDescription = ShortDescription;
        var oldNetContent = NetContent;
        var oldNetContentUom = NetContentUom;
        var oldPossibleCategories = PossibleCategories;
        var oldChildProductId = ChildProductId;
        var oldPublicationLifecycleId = PublicationLifecycleId;

        Name = update.Name ?? Name;
        BrandId = update.BrandId ?? BrandId;
        ExternalReferenceId = update.ExternalReferenceId ?? ExternalReferenceId;
        Variant = update.Variant ?? Variant;
        AltName = update.AltName ?? AltName;
        ShortDescription = update.ShortDescription ?? ShortDescription;
        NetContent = update.NetContent ?? NetContent;
        NetContentUom = update.NetContentUom ?? NetContentUom;
        PossibleCategories = update.PossibleCategories ?? PossibleCategories;
        ChildProductId = update.ChildProductId ?? ChildProductId;
        PublicationLifecycleId = update.PublicationLifecycleId ?? PublicationLifecycleId;

        if (oldName != Name ||
            oldBrandId != BrandId ||
            oldExternalReferenceId != ExternalReferenceId ||
            oldAltName != AltName ||
            oldVariant != Variant ||
            oldShortDescription != ShortDescription ||
            oldNetContent != NetContent ||
            oldNetContentUom != NetContentUom ||
            oldPossibleCategories != PossibleCategories ||
            oldChildProductId != ChildProductId ||
            oldPublicationLifecycleId != PublicationLifecycleId)
        {
            UpdatedBy = createdBy;
            UpdatedUtc = createdUtc;
        }
    }
}