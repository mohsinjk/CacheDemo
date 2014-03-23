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

        public NodeDTO GetNodeDtoByIdWithChildren(int id)
        {
            string key = CacheKey(id);
            NodeDTO nodeDTO = null;
            if (_cacheRepository.Exists(key))
            {
                _cacheRepository.Get(key, out nodeDTO);
                return nodeDTO;
            }
            else
            {
                Node node = base.GetAll().Where(x => x.Id == id).Include("Content").Include("Children").Include("Children.Content").ToList().First();
                Mapper.CreateMap<Node, NodeDTO>();
                nodeDTO = Mapper.Map<Node, NodeDTO>(node);

                _cacheRepository.Add(key, nodeDTO);
            }

            return nodeDTO;
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

                Mapper.CreateMap<Node, NodeDTO>();
                var nodeDTO = Mapper.Map<Node, NodeDTO>(entity);

                nodes.Children.Add(nodeDTO);
                _cacheRepository.Add(key, nodes);
            }
        }

        public override void Delete(Node entity)
        {
            if (entity.ParentId != null)
            {
                NodeDTO nodeDto;
                nodeDto = GetNodeDtoByIdWithChildren(entity.ParentId.Value);

                var deleteEntity = nodeDto.Children.Find(x => x.Id == entity.Id);
                nodeDto.Children.Remove(deleteEntity);
            }
            string key = string.Format("{0}.{1}", this.GetType().FullName, entity.Id);
            _cacheRepository.Clear(key);
            base.Delete(entity);
        }

        public override void Delete(int id)
        {
            string key = CacheKey(id);

            var node = GetNodeDtoByIdWithChildren(id);
            //Delete(node);
            _cacheRepository.Clear(key);
            base.Delete(id);
        }

        public void simpleDelete(Node entity)
        {
            base.Delete(entity);
        }


        //public override void Update(Node entity)
        //{
        //    string key = CacheKey(entity.Id);
        //    _cacheRepository.Add(key, entity);
        //    base.Update(entity);
        //}
    }

    public class NodeRepositoryWithCache : CacheRepository<Node>, INodeRepositoryWithCache
    {
        //public NodeRepositoryWithCache(ICacheRepository<Node> _cacheRepository)
        //{
        //    _cacheRepository = new CacheRepository<Node>();
        //}
        private string CacheKey(int id)
        {
            string key = string.Format("{0}.{1}", this.GetType().FullName, id);
            return key;
        }

        public bool GetNodeDtoByIdWithChildren(int id, out NodeDTO nodeDto)
        {
            string key = CacheKey(id);
            return base.Get(key, out nodeDto);
        }

        public void UpdateCacheForItsParent(NodeDTO nodeDto, NodeDTO parentNodeDto)
        {
            if (nodeDto.ParentId != null && parentNodeDto != null && parentNodeDto.Children != null)
            {
                string key = CacheKey(nodeDto.ParentId.Value);
                parentNodeDto.Children.Add(nodeDto);
                base.Add(key, parentNodeDto);
            }
            else
            {
                string key = CacheKey(nodeDto.Id);
                base.Add(key, nodeDto);
            }
        }

        public void DeleteCacheFromItsParent(NodeDTO nodeDto, NodeDTO parentNodeDto)
        {
            if (nodeDto.ParentId != null && parentNodeDto != null && parentNodeDto.Children != null)
            {
                string key = CacheKey(nodeDto.ParentId.Value); 

                var nodeDtoFromCache = parentNodeDto.Children.Find(x => x.Id == nodeDto.Id);
                parentNodeDto.Children.Remove(nodeDtoFromCache);
                
                base.Add(key, parentNodeDto);
            }
            else
            {
                string key = CacheKey(nodeDto.Id);
                base.Clear(key);
            }
        }
    }
}
