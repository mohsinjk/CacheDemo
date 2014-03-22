﻿using System.Collections.Generic;

namespace CacheDemo.Model
{
    public class Node
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public Node Parent { get; set; }
        public int ContentId { get; set; }
        public Content Content { get; set; }
        public List<Node> Children { get; set; }
    }
}