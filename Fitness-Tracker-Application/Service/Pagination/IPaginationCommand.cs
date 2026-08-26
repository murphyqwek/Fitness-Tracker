namespace Fitness_Tracker_Application.Service.Pagination
{
    public interface IPaginationCommand
    {
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
