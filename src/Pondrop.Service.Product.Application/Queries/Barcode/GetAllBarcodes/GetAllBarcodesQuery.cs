using MediatR;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Product.Domain.Models;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllBarcodesQuery : IRequest<Result<List<BarcodeEntity>>>
{
    public int Limit { get; set; }

    public int Offset { get; set; }
}