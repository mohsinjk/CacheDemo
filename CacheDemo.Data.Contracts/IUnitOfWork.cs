using System.Runtime.InteropServices;
using CacheDemo.Model;

namespace CacheDemo.Data.Contracts
{
    public interface IUnitOfWork
    {
        void Commit();

        INodeRepository Nodes { get; }
        INodeRepositoryWithCache NodesCache { get; }
        IRepository<Content> Contents { get; }
        IRepository<Portal> Portals { get; }
        IShortcutRepository Shortcuts { get; }
    }
}
