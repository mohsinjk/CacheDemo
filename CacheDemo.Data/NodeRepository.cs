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
                Node node = base.GetAll().Where(x => x.Id == id).Include("Content").Include("Children").Include("Children.Content").First();
                Mapper.CreateMap<Node, NodeDTO>();
                nodeDTO = Mapper.Map<Node, NodeDTO>(node);

                //nodeDTO = TransformFromNodeToNodeDTO(node);
                
                _cacheRepository.Add(key, nodeDTO);
            }

            return nodeDTO;
        }

        private string CacheKey(int id)
        {
            string key = string.Format("{0}.{1}", this.GetType().FullName, id);
            return key;
        }

        private NodeDTO TransformFromNodeToNodeDTO(Node node)
        {
            Mapper.CreateMap<Node, NodeDTO>();
            var d= Mapper.Map<Node, NodeDTO>(node);
            NodeDTO nodeDto = new NodeDTO();
            nodeDto.Id = node.Id;
            nodeDto.Description = node.Description;
            nodeDto.ContentId = node.ContentId;
            nodeDto.Content = node.Content;
            nodeDto.ParentId = node.ParentId;
            if (node.Parent != null)
            {
                nodeDto.Parent = new NodeDTO { Id = node.Parent.Id, Description = node.Parent.Description, ContentId = node.Parent.ContentId, Content = node.Parent.Content, ParentId = node.ParentId };
            }

            List<NodeDTO> nodeDTOlist = new List<NodeDTO>();
            if (node.Children != null)
            {
                foreach (var item in node.Children)
                {
                    NodeDTO obj = new NodeDTO();
                    obj.Id = item.Id;
                    obj.Description = item.Description;
                    obj.ContentId = item.ContentId;
                    obj.Content = item.Content;
                    obj.ParentId = item.ParentId;
                    nodeDTOlist.Add(obj);
                }
            }

            nodeDto.Children = nodeDTOlist;
            return nodeDto;
        }
        //public override Node GetById(int id)
        //{
        //    string key = string.Format("{0}.{1}.{2}", this.GetType().FullName, "GetById", id);
        //    Node node;
        //    if (_cacheRepository.Exists(key))
        //    {
        //        _cacheRepository.Get(key, out node);
        //        return node;
        //    }
        //    else
        //    {
        //        node = base.GetById(id);
        //        _cacheRepository.Add(key, node);
        //    }
        //    return node;
        //}

        public IEnumerable<Node> GetByParentId(int id)
        {
            string key = CacheKey(id);
            IEnumerable<Node> node;
            if (_cacheRepository.Exists(key + id))
            {
                _cacheRepository.Get(key + id, out node);
                return node;
            }
            else
            {
                node = base.GetAll().Where(x => x.ParentId == id).Include("Content").ToList();
                _cacheRepository.Add(key, node);
            }
            return node;
        }

        public void UpdateCacheForItsParent(Node entity)
        {
            if (entity.ParentId != null)
            {
                string key = CacheKey(entity.ParentId.Value);

                var parentId = entity.ParentId.Value;
                var nodes = GetNodeDtoByIdWithChildren(parentId);

                nodes.Children.Add(TransformFromNodeToNodeDTO(entity));
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

        //public override void Update(Node entity)
        //{
        //    string key = CacheKey(entity.Id);
        //    _cacheRepository.Add(key, entity);
        //    base.Update(entity);
        //}
    }
}
