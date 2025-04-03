using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using Subscription.Services.Exceptions;
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

    public async Task<bool> ConfirmSignUpAsync(ConfirmSignUpModel model)
    {
        var request = new ConfirmSignUpRequest
        {
            ClientId = _appClientId,
            SecretHash = CalculateSecretHash(model.Email),
            Username = model.Email,
            ConfirmationCode = model.ConfirmationCode
        };

        try
        {
            var finalResponse = await _client.ConfirmSignUpAsync(request);
            return finalResponse.HttpStatusCode == HttpStatusCode.OK;
        }
        catch(Exception e)
        {
            throw new CustomException(e);
        }
        
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

        try
        {
            var response = await _client.SignUpAsync(signUpRequest);

            return response.HttpStatusCode == HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            throw new CustomException(ex);
        }
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
            throw new CustomException(e);
        }
        catch (NotAuthorizedException e)
        {
            throw new CustomException(e);
        }
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordModel model)
    {
        try
        {
            var authRequest = new InitiateAuthRequest
            {
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                ClientId = _appClientId,
                AuthParameters = new Dictionary<string, string>
            {
                { "USERNAME", model.Email },
                { "PASSWORD", model.OldPassword },
                { "SECRET_HASH", CalculateSecretHash(model.Email)},
            }
            };

            var authResponse = await _client.InitiateAuthAsync(authRequest);

            if (authResponse.HttpStatusCode != HttpStatusCode.OK)
                return false;

            var passwordRequest = new AdminSetUserPasswordRequest
            {
                UserPoolId = _userPoolId,
                Username = model.Email,
                Password = model.NewPassword,
                Permanent = true
            };

            var passwordResponse = await _client.AdminSetUserPasswordAsync(passwordRequest);
            return passwordResponse.HttpStatusCode == HttpStatusCode.OK;
        }
        catch (NotAuthorizedException e)
        {
            throw new CustomException(e);
        }
        catch (Exception e)
        {
            throw new CustomException(e);
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
