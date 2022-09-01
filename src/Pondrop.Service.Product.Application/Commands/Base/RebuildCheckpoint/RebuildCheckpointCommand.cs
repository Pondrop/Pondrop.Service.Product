using MediatR;
using Pondrop.Service.Product.Application.Models;

namespace Pondrop.Service.Product.Application.Commands;

public abstract class RebuildCheckpointCommand : IRequest<Result<int>> 
{
}