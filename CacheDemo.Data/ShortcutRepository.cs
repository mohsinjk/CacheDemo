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
            var originalContent = base.GetAll().Where(x => x.LinkContentId == id).Include("OriginalContent").First();
            return originalContent.OriginalContent;
        }
        
    }
}
