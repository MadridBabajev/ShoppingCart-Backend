using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Helpers;

public static class IdentityHelpers
{
    // extension function
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        return Guid.Parse(
            user.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }

    public static bool ValidateToken(string jwt,  string key,
        string issuer, string audience, bool ignoreExpiration = true)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = GetValidationParameters(key, issuer, audience);

        SecurityToken validatedToken;
        try
        {
            var principal = tokenHandler.ValidateToken(jwt, validationParameters, out validatedToken);
        }
        catch (SecurityTokenExpiredException)
        {
            // is it ok to be expired? since we are refreshing expired jwt
            return ignoreExpiration;
        }
        catch (Exception)
        {
            // something else was wrong
            return false;
        }

        return true;
    }

    private static TokenValidationParameters GetValidationParameters(string key,
        string issuer, string audience)
    {
        return new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidIssuer = issuer,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true
        };
    }
    
}
