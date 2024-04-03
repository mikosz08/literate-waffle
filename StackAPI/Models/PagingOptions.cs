namespace StackAPI.Models
{
    public class PagingOptions
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 100;
        public string Order { get; set; } = "ASC";
        public string Sort { get; set; } = "Name";
    }

}

