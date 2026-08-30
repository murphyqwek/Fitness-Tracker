using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fitness_Tracker
{
    public static class UserClaimsExtension
    {
        public static Guid GetUserId(this ClaimsPrincipal user) 
        {
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var guid))
            {
                throw new UnauthorizedAccessException("Id пользователя отсутствует в токене");
            }

            return guid;
        }

        public static string GetUserLogin(this ClaimsPrincipal user)
        {
            var loginClaim = user.FindFirst(ClaimTypes.Name)?.Value
                  ?? user.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value
                  ?? user.FindFirst("unique_name")?.Value;

            if (string.IsNullOrEmpty(loginClaim))
            {
                throw new UnauthorizedAccessException("Login пользователя отсутствует в токене");
            }

            return loginClaim;
        }
    }
}
