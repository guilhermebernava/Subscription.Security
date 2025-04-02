using Subscription.Services.Models;

namespace Subscription.Services.Interfaces;
public interface IAuthServices
{
    Task<bool> CreateUserAsync(CreateUser model);
    Task<string> LoginAsync(Login model);
    Task<bool> ConfirmSignUpAsync(Login model);

    Task<bool> ResetPasswordAsync(Login model);

}
