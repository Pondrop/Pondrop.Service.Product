using Pondrop.Service.Product.Application.Interfaces.Services;

namespace Pondrop.Service.Product.Api.Services;

public class UserService : IUserService
{
    public string CurrentUserName() => "admin";
    public string GetMaterializedViewUserName() => "materialized_view";
}