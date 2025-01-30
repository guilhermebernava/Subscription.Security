using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using Subscription.Services.Interfaces;
using Subscription.Services.Models;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Subscription.Services.Services;
public class AuthServices : IAuthServices
{
    private readonly AmazonCognitoIdentityProviderClient _client;
    private readonly string _appClientId;
    private readonly string _appSecret;
    private readonly string _userPoolId;

    public AuthServices(IConfiguration configuration)
    {
        _client = new AmazonCognitoIdentityProviderClient(configuration["AWS:AccessKey"], configuration["AWS:AccessSecret"],
            Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"])); ;
        _appClientId = configuration["AWS:AppClientId"]!;
        _appSecret = configuration["AWS:AppClientSecret"]!;
        _userPoolId = configuration["AWS:UserPoolId"]!;
    }

    public async Task<bool> ConfirmSignUpAsync(Login model)
    {
        var request =  new AdminConfirmSignUpRequest
        {
            UserPoolId = _userPoolId,
            Username = model.Email
        };

        var finalResponse = await _client.AdminConfirmSignUpAsync(request);
        return finalResponse.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> CreateUserAsync(CreateUser model)
    {
        var signUpRequest = new SignUpRequest
        {
            ClientId = _appClientId,
            Username = model.Email,
            Password = model.Password,
            SecretHash = CalculateSecretHash(model.Email),
            UserAttributes = new List<AttributeType>
            {
                new AttributeType { Name = "email", Value = model.Email }
            }
        };

        var response = await _client.SignUpAsync(signUpRequest);

        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<string> LoginAsync(Login model)
    {
        var authRequest = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            ClientId = _appClientId,
            AuthParameters = new Dictionary<string, string>
            {
                { "USERNAME", model.Email },
                { "PASSWORD", model.Password },
                { "SECRET_HASH", CalculateSecretHash(model.Email)},
            }
        };

        try
        {
            var authResponse = await _client.InitiateAuthAsync(authRequest);
            return authResponse.AuthenticationResult.IdToken;
        }
        catch (UserNotConfirmedException e)
        {
            return "User not confirmed yet, change the password";
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    private string CalculateSecretHash(string email)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_appSecret);
        var messageBytes = Encoding.UTF8.GetBytes($"{email}{_appClientId}");

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);

        return Convert.ToBase64String(hashBytes);
    }

}
