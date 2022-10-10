using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateBarcodeCommand : IRequest<Result<BarcodeRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public string? BarcodeNumber { get; init; } = null;
    public string? BarcodeText { get; init; } = null;
    public string? BarcodeType { get; init; } = null;
    public Guid? ProductId { get; init; } = null;
    public Guid? RetailerId { get; init; } = null;
    public Guid? CompanyId { get; init; } = null;
    public string? PublicationLifecycleId { get; init; } = null;
}