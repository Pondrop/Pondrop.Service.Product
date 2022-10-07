using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateBrandCommand : IRequest<Result<BrandRecord>>
{
    public Guid Id { get; init; } = Guid.Empty;
    public string? Name { get; init; } = null;
    public Guid? CompanyId { get; init; } = null;
    public string? PublicationLifecycleId { get; init; } = null;
}