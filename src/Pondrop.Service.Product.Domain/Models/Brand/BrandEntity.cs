using Newtonsoft.Json;
using Pondrop.Service.Product.Domain.Events;
using Pondrop.Service.Product.Domain.Events.Brand;

namespace Pondrop.Service.Product.Domain.Models;

public record BrandEntity : EventEntity
{
    public BrandEntity()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        CompanyId = Guid.Empty;
        PublicationLifecycleId = string.Empty;
    }

    public BrandEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public BrandEntity(string name, Guid companyId, string publicationLifecycleId, string createdBy) : this()
    {
        var create = new CreateBrand(Guid.NewGuid(), name, companyId, publicationLifecycleId);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; private set; }

    [JsonProperty(PropertyName = "companyId")]
    public Guid CompanyId { get; private set; }

    [JsonProperty(PropertyName = "publicationLifecycleID")]
    public string PublicationLifecycleId { get; private set; }


    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateBrand create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateBrand update:
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
        if (eventPayloadToApply is CreateBrand create)
        {
            Apply(new Event(GetStreamId<BrandEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateBrand create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        Name = create.Name;
        CompanyId = create.CompanyId;
        PublicationLifecycleId = create.PublicationLifecycleId;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateBrand update, string createdBy, DateTime createdUtc)
    {
        var oldName = Name;
        var oldCompanyId = CompanyId;
        var oldPublicationLifecycleID = PublicationLifecycleId;

        Name = update.Name ?? Name;
        CompanyId = update.CompanyId ?? CompanyId;
        PublicationLifecycleId = update.PublicationLifecycleId ?? PublicationLifecycleId;

        if (oldName != Name ||
            oldCompanyId != CompanyId ||
            oldPublicationLifecycleID != PublicationLifecycleId)
        {
            UpdatedBy = createdBy;
            UpdatedUtc = createdUtc;
        }
    }
}