namespace StackAPI.Models
{
    public class StackTag
    {
        public required string Name { get; set; }
        public int Count { get; set; }
        public bool HasSynonyms { get; set; }
        public bool IsModeratorOnly { get; set; }
        public bool IsRequired { get; set; }
    }
}
