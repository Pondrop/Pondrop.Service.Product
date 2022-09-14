using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllStoresQueryHandlerValidator : AbstractValidator<GetAllCategoriesQuery>
{
    public GetAllStoresQueryHandlerValidator()
    {
    }
}