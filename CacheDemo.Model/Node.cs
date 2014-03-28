using System.Collections.Generic;

namespace CacheDemo.Model
{
    public class Node
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public virtual Node Parent { get; set; }
        public int ContentId { get; set; }
        public virtual Content Content { get; set; }
        public virtual ICollection<Node> Children { get; set; }
    }

    public class NodeWithOneLevelChildern
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public NodeWithOneLevelChildern Parent { get; set; }
        public int ContentId { get; set; }
        public Content Content { get; set; }
        public List<NodeWithOneLevelChildern> Children { get; set; }
    }
}
