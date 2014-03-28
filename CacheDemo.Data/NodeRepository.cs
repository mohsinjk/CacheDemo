using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CacheDemo.Data.Contracts;
using CacheDemo.Model;
using AutoMapper;

namespace CacheDemo.Data
{
    public class NodeRepository : Repository<Node>, INodeRepository
    {
        private readonly ICacheRepository<Node> _cacheRepository;
        public NodeRepository(DbContext context)
            : base(context)
        {
            _cacheRepository = new CacheRepository<Node>();
        }
        public override Node GetById(int id)
        {
            return base.GetAll().Where(x => x.Id == id).Include("Content").Include("Children").Include("Children.Content").First();
        }

        public Node GetByContentId(int contentId)
        {
            return base.GetAll().Where(x => x.Id == contentId).First();
        }

        public NodeWithOneLevelChildern GetNodeDtoByIdWithChildren(int id)
        {
            string key = CacheKey(id);
            NodeWithOneLevelChildern nodeWithOneLevelChildern = null;
            if (_cacheRepository.Exists(key))
            {
                _cacheRepository.Get(key, out nodeWithOneLevelChildern);
                return nodeWithOneLevelChildern;
            }
            else
            {
                Node node = base.GetAll().Where(x => x.Id == id).Include("Content").Include("Children").Include("Children.Content").ToList().First();
                Mapper.CreateMap<Node, NodeWithOneLevelChildern>();
                nodeWithOneLevelChildern = Mapper.Map<Node, NodeWithOneLevelChildern>(node);

                _cacheRepository.Add(key, nodeWithOneLevelChildern);
            }

            return nodeWithOneLevelChildern;
        }

        private string CacheKey(int id)
        {
            string key = string.Format("{0}.{1}", this.GetType().FullName, id);
            return key;
        }

        public void UpdateCacheForItsParent(Node entity)
        {
            if (entity.ParentId != null)
            {
                string key = CacheKey(entity.ParentId.Value);

                var parentId = entity.ParentId.Value;
                var nodes = GetNodeDtoByIdWithChildren(parentId);

                Mapper.CreateMap<Node, NodeWithOneLevelChildern>();
                var nodeDTO = Mapper.Map<Node, NodeWithOneLevelChildern>(entity);

                nodes.Children.Add(nodeDTO);
                _cacheRepository.Add(key, nodes);
            }
        }

        public override void Delete(Node entity)
        {
            if (entity.ParentId != null)
            {
                NodeWithOneLevelChildern nodeWithOneLevelChildern;
                nodeWithOneLevelChildern = GetNodeDtoByIdWithChildren(entity.ParentId.Value);

                var deleteEntity = nodeWithOneLevelChildern.Children.Find(x => x.Id == entity.Id);
                nodeWithOneLevelChildern.Children.Remove(deleteEntity);
            }
            string key = string.Format("{0}.{1}", this.GetType().FullName, entity.Id);
            _cacheRepository.Clear(key);
            base.Delete(entity);
        }

        public override void Delete(int id)
        {
            string key = CacheKey(id);

            var node = GetNodeDtoByIdWithChildren(id);
            _cacheRepository.Clear(key);
            base.Delete(id);
        }

        public void simpleDelete(Node entity)
        {
            base.Delete(entity);
        }

    }

    public class NodeRepositoryWithCache : CacheRepository<Node>, INodeRepositoryWithCache
    {
        private string CacheKey(int id)
        {
            string key = string.Format("{0}.{1}", this.GetType().FullName, id);
            return key;
        }

        public bool GetNodeDtoByIdWithChildren(int id, out NodeWithOneLevelChildern nodeWithOneLevelChildern)
        {
            string key = CacheKey(id);
            return base.Get(key, out nodeWithOneLevelChildern);
        }

        public void UpdateCacheForItsParent(NodeWithOneLevelChildern nodeWithOneLevelChildern, NodeWithOneLevelChildern parentNodeWithOneLevelChildern)
        {
            if (nodeWithOneLevelChildern.ParentId != null && parentNodeWithOneLevelChildern != null && parentNodeWithOneLevelChildern.Children != null)
            {
                string key = CacheKey(nodeWithOneLevelChildern.ParentId.Value);
                parentNodeWithOneLevelChildern.Children.Add(nodeWithOneLevelChildern);
                base.Add(key, parentNodeWithOneLevelChildern);
            }
            else
            {
                string key = CacheKey(nodeWithOneLevelChildern.Id);
                base.Add(key, nodeWithOneLevelChildern);
            }
        }

        public void DeleteCacheFromItsParent(NodeWithOneLevelChildern nodeWithOneLevelChildern, NodeWithOneLevelChildern parentNodeWithOneLevelChildern)
        {
            if (nodeWithOneLevelChildern.ParentId != null && parentNodeWithOneLevelChildern != null && parentNodeWithOneLevelChildern.Children != null)
            {
                string key = CacheKey(nodeWithOneLevelChildern.ParentId.Value); 

                var nodeDtoFromCache = parentNodeWithOneLevelChildern.Children.Find(x => x.Id == nodeWithOneLevelChildern.Id);
                parentNodeWithOneLevelChildern.Children.Remove(nodeDtoFromCache);
                
                base.Add(key, parentNodeWithOneLevelChildern);
            }
            else
            {
                string key = CacheKey(nodeWithOneLevelChildern.Id);
                base.Clear(key);
            }
        }
    }
}
