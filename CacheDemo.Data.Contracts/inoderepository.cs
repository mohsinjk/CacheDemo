using System.Collections.Generic;
using CacheDemo.Model;

namespace CacheDemo.Data.Contracts
{
    public interface INodeRepository:IRepository<Node>
    {
        Node GetByIdChildrenAndShortcut(int id);
        //IEnumerable<Node> GetByParentId(int id);
        //void AddAndUpdateCacheForItsParent(Node node);
    }
}
