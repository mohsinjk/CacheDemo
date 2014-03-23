using System.Collections.Generic;
using CacheDemo.Model;

namespace CacheDemo.Data.Contracts
{
    public interface IShortcutRepository:IRepository<Shortcut>
    {
        Shortcut GetOriginalContentByLinkContentId(int linkContentId);
    }
}
