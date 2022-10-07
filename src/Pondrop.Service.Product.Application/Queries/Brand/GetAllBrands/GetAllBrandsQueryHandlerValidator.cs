using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllBrandsQueryHandlerValidator : AbstractValidator<GetAllBrandsQuery>
{
    public GetAllBrandsQueryHandlerValidator()
    {
    }
}