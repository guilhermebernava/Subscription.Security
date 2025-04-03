namespace Subscription.Security.Exceptions;

public class UserIncorrectException :Exception
{
    public UserIncorrectException(string message) : base(message) { }
}
