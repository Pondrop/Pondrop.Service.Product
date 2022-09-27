using MediatR;
using Pondrop.Service.Product.Application.Commands;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.ProductCategory.Domain.Models;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateProductCategoryCheckpointByIdCommand : UpdateCheckpointByIdCommand<Result<ProductCategoryRecord>>
{
}