namespace Fitness_Tracker_Application.Service.Pagination
{
    public class PaginationResponse<T>
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public int Total { get; set; }
        public IList<T> Data { get; set; }

        public PaginationResponse(int page, int size, int total, IList<T> data)
        {
            Page = page;
            Size = size;
            Total = total;
            Data = data;
        }
    }
}
