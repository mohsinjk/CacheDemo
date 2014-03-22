namespace CacheDemo.Model
{
    public class Shortcut
    {
        public int Id { get; set; }
        public int LinkContentId { get; set; }
        public Content LinkContent { get; set; }
        public int OriginalContentId { get; set; }
        public Content OriginalContent { get; set; }
    }
}