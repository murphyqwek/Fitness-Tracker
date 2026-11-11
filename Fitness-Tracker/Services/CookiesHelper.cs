namespace Fitness_Tracker.Services
{
    using Fitness_Tracker_Application.Features.Users.Refresh;

    public static class CookiesHelper
    {
        public static void SetAccessAndRefreshTokenCookies(HttpResponse response, string access, string refresh)
        {
            SetAccessTokenCookie(response, access);
            SetRefreshTokenCookie(response, refresh);
        }

        public static void SetAccessTokenCookie(HttpResponse response, string token)
        {
            response.Cookies.Append("accessToken", token, new CookieOptions
            {
                Secure = true,
                SameSite = SameSiteMode.Strict,
                HttpOnly = true,
            });
        }

        public static void SetRefreshTokenCookie(HttpResponse response, string refreshToken)
        {
            response.Cookies.Append("refreshToken", refreshToken, new CookieOptions()
            {
                Secure = true,
                SameSite = SameSiteMode.Strict,
                HttpOnly = true,
                Path = "/api/v1/auth/refresh",
                MaxAge = RefreshTokenPolicy.Lifetime,
                Expires = DateTimeOffset.UtcNow.Add(RefreshTokenPolicy.Lifetime)
            });
        }
    }
}
