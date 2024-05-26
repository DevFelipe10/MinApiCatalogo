using MinApiCatalogo.Models;

namespace MinApiCatalogo.Services;

public interface ITokenService
{
    string GerarToken(string key, string issuer, string audience, UserModel user);
}
