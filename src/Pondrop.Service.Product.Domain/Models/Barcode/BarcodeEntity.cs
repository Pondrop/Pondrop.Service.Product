using Newtonsoft.Json;
using Pondrop.Service.Product.Domain.Events;
using Pondrop.Service.Product.Domain.Events.Barcode;

namespace Pondrop.Service.Product.Domain.Models;

public record BarcodeEntity : EventEntity
{
    public BarcodeEntity()
    {
        Id = Guid.Empty;
        BarcodeNumber = string.Empty;
        BarcodeText = string.Empty;
        BarcodeType = string.Empty;
        ProductId = Guid.Empty;
        RetailerId = Guid.Empty;
        CompanyId = Guid.Empty;
        PublicationLifecycleId = string.Empty;
    }

    public BarcodeEntity(IEnumerable<IEvent> events) : this()
    {
        foreach (var e in events)
        {
            Apply(e);
        }
    }

    public BarcodeEntity(string barcodeNumber, string barcodeText, string barcodeType, Guid productId, Guid retailerId, Guid companyId, string publicationLifecycleId, string createdBy) : this()
    {
        var create = new CreateBarcode(Guid.NewGuid(), barcodeNumber, barcodeText, barcodeType, productId, retailerId, companyId, publicationLifecycleId);
        Apply(create, createdBy);
    }

    [JsonProperty(PropertyName = "barcodeNumber")]
    public string BarcodeNumber { get; private set; }

    [JsonProperty(PropertyName = "barcodeText")]
    public string BarcodeText { get; private set; }

    [JsonProperty(PropertyName = "barcodeType")]
    public string BarcodeType { get; private set; }


    [JsonProperty(PropertyName = "productId")]
    public Guid ProductId { get; private set; }


    [JsonProperty(PropertyName = "retailerId")]
    public Guid RetailerId { get; private set; }


    [JsonProperty(PropertyName = "companyId")]
    public Guid CompanyId { get; private set; }

    [JsonProperty(PropertyName = "publicationLifecycleID")]
    public string PublicationLifecycleId { get; private set; }


    protected sealed override void Apply(IEvent eventToApply)
    {
        switch (eventToApply.GetEventPayload())
        {
            case CreateBarcode create:
                When(create, eventToApply.CreatedBy, eventToApply.CreatedUtc);
                break;
            case UpdateBarcode update:
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
        if (eventPayloadToApply is CreateBarcode create)
        {
            Apply(new Event(GetStreamId<BarcodeEntity>(create.Id), StreamType, 0, create, createdBy));
        }
        else
        {
            Apply(new Event(StreamId, StreamType, AtSequence + 1, eventPayloadToApply, createdBy));
        }
    }

    private void When(CreateBarcode create, string createdBy, DateTime createdUtc)
    {
        Id = create.Id;
        BarcodeNumber = create.BarcodeNumber;
        BarcodeText = create.BarcodeText;
        BarcodeType = create.BarcodeType;
        ProductId = create.ProductId;
        RetailerId = create.RetailerId;
        CompanyId = create.CompanyId;
        PublicationLifecycleId = create.PublicationLifecycleId;
        CreatedBy = UpdatedBy = createdBy;
        CreatedUtc = UpdatedUtc = createdUtc;
    }

    private void When(UpdateBarcode update, string createdBy, DateTime createdUtc)
    {
        var oldBarcodeNumber = BarcodeNumber;
        var oldBarcodeText = BarcodeText;
        var oldBarcodeType = BarcodeType;
        var oldProductId = ProductId;
        var oldRetailerId = RetailerId;
        var oldCompanyId = CompanyId;
        var oldPublicationLifecycleID = PublicationLifecycleId;


        BarcodeNumber = update.BarcodeNumber ?? BarcodeNumber;
        BarcodeText = update.BarcodeText ?? BarcodeText;
        BarcodeType = update.BarcodeType ?? BarcodeType;
        ProductId = update.ProductId ?? ProductId;
        RetailerId = update.RetailerId ?? RetailerId;
        CompanyId = update.CompanyId ?? CompanyId;
        PublicationLifecycleId = update.PublicationLifecycleId ?? PublicationLifecycleId;

        if (oldBarcodeNumber != BarcodeNumber ||
            oldBarcodeText != BarcodeText ||
            oldBarcodeType != BarcodeType ||
            oldProductId != ProductId ||
            oldRetailerId != RetailerId ||
            oldCompanyId != CompanyId ||
            oldPublicationLifecycleID != PublicationLifecycleId)
        {
            UpdatedBy = createdBy;
            UpdatedUtc = createdUtc;
        }
    }
}