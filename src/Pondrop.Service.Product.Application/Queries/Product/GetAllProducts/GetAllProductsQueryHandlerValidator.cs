using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllProductsQueryHandlerValidator : AbstractValidator<GetAllProductsQuery>
{
    public GetAllProductsQueryHandlerValidator()
    {
    }
}