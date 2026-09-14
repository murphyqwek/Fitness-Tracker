using System.Security.Claims;

namespace Fintess_Tracker_Analytics
{
    public static class UserClaimExtension
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
    }
}
