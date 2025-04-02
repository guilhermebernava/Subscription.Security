namespace Subscription.Services.Models;

public class Login
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string? NewPassword { get; set; }
}
