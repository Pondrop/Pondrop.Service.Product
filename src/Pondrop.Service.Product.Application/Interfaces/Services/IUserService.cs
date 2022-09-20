using Pondrop.Service.Product.Domain.Models.User;

namespace Pondrop.Service.Product.Application.Interfaces.Services;

public interface IUserService
{
    string GetMaterializedViewUserName();

    string CurrentUserId();

    string CurrentUserName();

    UserType CurrentUserType();

    bool SetCurrentUser(UserModel user);
}