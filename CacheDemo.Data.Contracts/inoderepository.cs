using System.Collections.Generic;
using CacheDemo.Model;

namespace CacheDemo.Data.Contracts
{
    public interface INodeRepository:IRepository<Node>
    {
        NodeDTO GetNodeDtoByIdWithChildren(int id);
        //IEnumerable<Node> GetByParentId(int id);
        void UpdateCacheForItsParent(Node node);
        Node GetByContentId(int contentId);
        void simpleDelete(Node entity);
    }

    public interface INodeRepositoryWithCache
    {
        bool GetNodeDtoByIdWithChildren(int id, out NodeDTO nodeDto);
        void UpdateCacheForItsParent(NodeDTO nodeDto, NodeDTO parentNodeDto);
        void DeleteCacheFromItsParent(NodeDTO nodeDto, NodeDTO parentNodeDto);
    }
}
