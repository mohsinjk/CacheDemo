using System;
using System.Linq;
using System.Runtime.Caching;
using CacheDemo.Data.Contracts;

namespace CacheDemo.Data
{
    public class CacheRepository<T> : ICacheRepository<T> where T:class
    {
        private static readonly object CacheLock = new object();
        private static ObjectCache cache = MemoryCache.Default;
        public IQueryable<T> AsQueryable()
        {
            throw new NotImplementedException();
            //var result = (T)obj.Get(key) as IQueryable<T>;

            //if (result == null)
            //{
            //    lock (CacheLock)
            //    {
            //        result = (T)obj.Get(key) as IQueryable<T>;
            //        if (result == null)
            //        {
            //            result = this.iDbContext.Set<T>().AsQueryable();
            //            obj.Add(key, result);
            //        }
            //    }
            //}

            //return result;
        }

        /// <summary>
        /// Insert value into the cache using
        /// appropriate name/value pairs
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="o">Item to be cached</param>
        /// <param name="key">Name of item</param>
        /// <param name="value"></param>
        public void Add<T>(string key, T value)
        {
            // NOTE: Apply expiration parameters as you see fit.
            // I typically pull from configuration file.

            // In this example, I want an absolute
            // timeout so changes will always be reflected
            // at that time. Hence, the NoSlidingExpiration.
            if (!Exists(key))
                cache.Add(key, value, null);
            else
            {
                Clear(key);
                cache.Add(key, value, null);
            }
        }

        /// <summary>
        /// Retrieve cached item
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Name of cached item</param>
        /// <param name="value">Cached value. Default(T) if
        /// item doesn't exist.</param>
        /// <returns>Cached item as type</returns>
        public bool Get<T>(string key, out T value)
        {
            try
            {
                if (!Exists(key))
                {
                    value = default(T);
                    return false;
                }

                value = (T)cache.Get(key);
            }
            catch
            {
                value = default(T);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Remove item from cache
        /// </summary>
        /// <param name="key">Name of cached item</param>
        public void Clear(string key)
        {
            if (Exists(key))
                cache.Remove(key);
        }

        /// <summary>
        /// Check for item in cache
        /// </summary>
        /// <param name="key">Name of cached item</param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            return cache.Contains(key);
        }

    }
}