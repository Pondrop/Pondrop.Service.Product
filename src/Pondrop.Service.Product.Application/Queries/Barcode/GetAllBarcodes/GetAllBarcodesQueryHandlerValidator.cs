using FluentValidation;

namespace Pondrop.Service.Product.Application.Queries;

public class GetAllBarcodesQueryHandlerValidator : AbstractValidator<GetAllBarcodesQuery>
{
    public GetAllBarcodesQueryHandlerValidator()
    {
    }
}