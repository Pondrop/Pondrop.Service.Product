using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateBarcodeCommand : IRequest<Result<BarcodeRecord>>
{
    public string BarcodeNumber { get;init; } = string.Empty;
    public string BarcodeText { get;init; } = string.Empty;
    public string BarcodeType { get;init; } = string.Empty;
    public Guid ProductId { get;init; } = Guid.Empty;
    public Guid RetailerId { get;init; } = Guid.Empty;
    public Guid CompanyId { get;init; } = Guid.Empty;
    public string PublicationLifecycleId { get; init; } = string.Empty;
}
