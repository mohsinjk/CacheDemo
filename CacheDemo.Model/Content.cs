namespace CacheDemo.Model
{
    public class Content
    {
        public int Id { get; set; }
        public int PortalId { get; set; }
        public Portal Portal { get; set; }
        public ContentType Type { get; set; }
    }

    public enum ContentType
    {
        Original = 1,
        Shortcut = 2
    }
}