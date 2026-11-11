namespace Fitness_Tracker_Application.Features.Users.Refresh
{
    public static class RefreshTokenPolicy
    {
        public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);
    }
}
