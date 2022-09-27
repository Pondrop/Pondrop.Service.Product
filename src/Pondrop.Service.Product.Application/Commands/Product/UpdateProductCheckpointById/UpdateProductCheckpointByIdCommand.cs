using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;
using Pondrop.Service.Product.Domain.Models.Product;

namespace Pondrop.Service.Product.Application.Commands;

public class UpdateProductCheckpointByIdCommand : UpdateCheckpointByIdCommand<Result<ProductRecord>>
{
}