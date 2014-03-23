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
        public Shortcut GetOriginalContentByLinkContentId(int linkContentId)
        {
            var originalContent = base.GetAll().Where(x => x.LinkContentId == linkContentId).Include("OriginalContent").First();
            return originalContent;
        }
        
    }
}
