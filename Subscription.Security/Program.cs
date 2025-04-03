using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Subscription.Security.Middlewares;
using Subscription.Services.Interfaces;
using Subscription.Services.Models;
using Subscription.Services.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped<IAuthServices, AuthServices>();

var myAllowedOrigins = "_myAllowedOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowedOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["AWS:CognitoUrl"];
        options.Audience = builder.Configuration["AWS:AppClientId"];
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors(myAllowedOrigins);
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CustomExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/createUser", async ([FromBody] CreateUser model, [FromServices] IAuthServices services) =>
{
    var result = await services.CreateUserAsync(model);
    return Results.Ok(result);
});

app.MapPost("/login", async ([FromBody] Login model, [FromServices] IAuthServices services) =>
{
    var result = await services.LoginAsync(model);
    return Results.Ok(result);
});

app.MapPost("/confirmSignUp", async ([FromBody] ConfirmSignUpModel model, [FromServices] IAuthServices services) =>
{
    var result = await services.ConfirmSignUpAsync(model);
    if (!result) Results.BadRequest();
    return Results.Ok(result);
});


app.MapPost("/resetPassword", async ([FromBody] ResetPasswordModel model, [FromServices] IAuthServices services) =>
{
    var result = await services.ResetPasswordAsync(model);
    if (!result) Results.BadRequest();
    return Results.Ok(result);
});

app.UseHttpsRedirection();

app.Run();