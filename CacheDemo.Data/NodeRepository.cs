using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CacheDemo.Data.Contracts;
using CacheDemo.Model;

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
        public Node GetByIdChildrenAndShortcut(int id)
        {
            Node node = base.GetAll().Where(x => x.Id == id).Include("Content").First();
            var childNode = base.GetAll().Where(x => x.ParentId == id).Include("Content").ToList();
        
            node.Children = childNode;
            return node;
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

        //public IEnumerable<Node> GetByParentId(int id)
        //{
        //    string key = string.Format("{0}.{1}.{2}", this.GetType().FullName, "GetById", id);
        //    IEnumerable<Node> node;
        //    if (_cacheRepository.Exists(key + id))
        //    {
        //        _cacheRepository.Get(key + id, out node);
        //        return node;
        //    }
        //    else
        //    {
        //        node = base.GetAll().Where(x => x.ParentId == id).Include("Content").ToList();
        //        _cacheRepository.Add(key, node);
        //    }
        //    return node;
        //}

        //public void AddAndUpdateCacheForItsParent(Node entity)
        //{
        //    if (entity.ParentId != null)
        //    {
        //        var parentId = entity.ParentId.Value;
        //        var nodes = GetByParentId(parentId).ToList();
        //        //var nodes = GetById(parentId).Children.ToList();
        //        nodes.Add(entity);
        //        //string key = string.Format("{0}.{1}.{2}", this.GetType().FullName, "GetByParentId", parentId);
        //        string key = string.Format("{0}.{1}.{2}", this.GetType().FullName, "GetById", entity.ParentId.Value);
        //        _cacheRepository.Add(key, nodes);
        //    }
        //    base.Add(entity);
        //}

        //public override void Delete(Node entity)
        //{
        //    if (entity.ParentId != null)
        //    {
        //        string key = string.Format("{0}.{1}.{2}", this.GetType().FullName, "GetById", entity.ParentId.Value);
        //        Node node;
        //        _cacheRepository.Get(key, out node);

        //        var deleteEntity= node.Children.Find(x => x.Id == entity.Id);
        //        node.Children.Remove(deleteEntity);
        //    }
        //    base.Delete(entity);
        //}

        //public override void Delete(int id)
        //{
        //    string key = string.Format("{0}.{1}.{2}", this.GetType().FullName, "GetById", id);
        //    _cacheRepository.Clear(key);
        //    base.Delete(id);
        //}

        //public override void Update(Node entity)
        //{
        //    string key = string.Format("{0}.{1}.{2}", this.GetType().FullName, "GetById", entity.Id);
        //    _cacheRepository.Add(key, entity);
        //    base.Update(entity);
        //}
    }
}
