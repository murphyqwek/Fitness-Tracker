namespace Fitness_Tracker_Application.Features.Users.Registration
{
    [Serializable]
    internal class DbUpdateException : Exception
    {
        public DbUpdateException()
        {
        }

        public DbUpdateException(string? message) : base(message)
        {
        }

        public DbUpdateException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}