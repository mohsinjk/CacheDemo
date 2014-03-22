using System.Collections.Generic;
using CacheDemo.Model;

namespace CacheDemo.Data.Contracts
{
    public interface INodeRepository:IRepository<Node>
    {
        NodeDTO GetNodeDtoByIdWithChildren(int id);
        IEnumerable<Node> GetByParentId(int id);
        void UpdateCacheForItsParent(Node node);
    }
}
