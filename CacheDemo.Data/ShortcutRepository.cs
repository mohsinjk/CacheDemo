using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CacheDemo.Data.Contracts;
using CacheDemo.Model;

namespace CacheDemo.Data
{
    public class ShortcutRepository : Repository<Shortcut>, IShortcutRepository
    {
        private readonly ICacheRepository<Node> _cacheRepository;
        public ShortcutRepository(DbContext context)
            : base(context)
        {
            _cacheRepository = new CacheRepository<Node>();
        }
        public Content GetOriginalContentByLinkContentId(int id)
        {
            //We do not need to cache Shortcuts Content because Node Cache is already caching.

            //string key = string.Format("{0}.{1}.", this.GetType().FullName, id);
            //Shortcut shortcut;
            //if (_cacheRepository.Exists(key))
            //{
            //    _cacheRepository.Get(key, out shortcut);
            //    return shortcut.OriginalContent;
            //}
            //else
            //{
            //    shortcut = base.GetAll().Where(x => x.LinkContentId == id).Include("OriginalContent").First();
            //    _cacheRepository.Add(key, shortcut);
            //}
            //return shortcut.OriginalContent;
            var originalContent = base.GetAll().Where(x => x.LinkContentId == id).Include("OriginalContent").First();
            return originalContent.OriginalContent;
        }
        
    }
}
