using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class CreateBrandCommand : IRequest<Result<BrandRecord>>
{
    public string Name { get; init; } = string.Empty;

    public Guid CompanyId { get; init; } = Guid.Empty;

    public string WebsiteUrl { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string PublicationLifecycleId { get; init; } = string.Empty;
}
