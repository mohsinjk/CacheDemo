using System.Collections.Generic;
using CacheDemo.Model;

namespace CacheDemo.Data.Contracts
{
    public interface INodeRepository:IRepository<Node>
    {
        NodeWithOneLevelChildern GetNodeDtoByIdWithChildren(int id);
        //IEnumerable<Node> GetByParentId(int id);
        void UpdateCacheForItsParent(Node node);
        Node GetByContentId(int contentId);
        void simpleDelete(Node entity);
    }

    public interface INodeRepositoryWithCache
    {
        bool GetNodeDtoByIdWithChildren(int id, out NodeWithOneLevelChildern nodeWithOneLevelChildern);
        void UpdateCacheForItsParent(NodeWithOneLevelChildern nodeWithOneLevelChildern, NodeWithOneLevelChildern parentNodeWithOneLevelChildern);
        void DeleteCacheFromItsParent(NodeWithOneLevelChildern nodeWithOneLevelChildern, NodeWithOneLevelChildern parentNodeWithOneLevelChildern);
    }
}
