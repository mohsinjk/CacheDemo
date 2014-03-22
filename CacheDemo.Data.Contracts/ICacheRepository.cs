using System.Collections.Generic;
using System.Linq;

namespace CacheDemo.Data.Contracts
{
    public interface ICacheRepository<out T> where T: class
    {
        IQueryable<T> AsQueryable();
        void Add<T>(string key, T value);
        bool Get<T>(string key, out T value);
        void Clear(string key);
        bool Exists(string key);
    }

    public interface ICachedRepository<out T> where T : class
    {
        IQueryable<T> AsQueryable();
        IEnumerable<T> GetAll();
    }
}
