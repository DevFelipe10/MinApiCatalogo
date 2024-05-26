using Microsoft.AspNetCore.Authorization;
using MinApiCatalogo.Models;
using MinApiCatalogo.Services;

namespace MinApiCatalogo.ApiEndpoints;

public static class AutenticacaoEndpoints
{
    public static void MapAutenticacaoEndpoints(this WebApplication app)
    {
        // endpoint para login
        app.MapPost("/login", [AllowAnonymous] (UserModel user, ITokenService tokenService) =>
        {
            if (user == null)
            {
                return Results.BadRequest("Login Inválido");
            }

            if (user.UserName == "felipe" && user.Password == "teste123")
            {
                var tokenString = tokenService.GerarToken(app.Configuration["Jwt:key"],
                    app.Configuration["Jwt:Issuer"],
                    app.Configuration["Jwt:Audience"],
                    user);

                return Results.Ok(new { token = tokenString });
            }

            return Results.BadRequest("Login Inválido");
        }).Produces(StatusCodes.Status400BadRequest)
          .Produces(StatusCodes.Status200OK)
          .WithName("Login")
          .WithTags("Autenticação");
    }
}
