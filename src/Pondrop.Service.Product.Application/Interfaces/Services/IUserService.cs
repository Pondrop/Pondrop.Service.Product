namespace Pondrop.Service.Product.Application.Interfaces.Services;

public interface IUserService
{
    string CurrentUserName();
    string GetMaterializedViewUserName();
}