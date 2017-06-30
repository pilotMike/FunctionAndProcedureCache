using GlobalCache.Tracking;
using System;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GlobalCache.Tests")]
namespace GlobalCache.Caching
{
    /// <summary>
    /// Simple wrapper around the .NET MemoryCache with cache miss and request count tracking.
    /// Default Cache Policy for cache items is a 1 hour sliding window.
    /// </summary>
    public abstract class BasicMemoryCache 
    {
        #region Fields

        private readonly MemoryCache _cache;
        private readonly TimeSpan DefaultSlidingExpiration = TimeSpan.FromHours(1);
        private HitCountTrack _hitCount;

        #endregion

        #region Properties

        /// <summary>
        /// Cache item policy to use for the stored procedure lookup. 
        /// Default is a sliding time scale of one hour.
        /// </summary>
        public CacheItemPolicy DefaultCachePolicy { get; private set; }
        public bool EnableHitCount { get; set; }
        public HitCountTrack HitCount => _hitCount ?? (_hitCount = new HitCountTrack());

        #endregion

        #region Constructor

        public BasicMemoryCache()
        {
            _cache = MemoryCache.Default;
            DefaultCachePolicy = new CacheItemPolicy() { SlidingExpiration = DefaultSlidingExpiration };
        }

        internal BasicMemoryCache(MemoryCache cache)
        {
            _cache = cache;
        }

        #endregion

        #region Methods

        protected T Get<T>(string key, Func<T> request, CacheItemPolicy cacheItemPolicy = null)
        {
            T result;
            if (!TryGetValue(key, out result))
            {
                result = request();
                Add(key, result, cacheItemPolicy);
            }
            return result;
        }

        protected bool TryGetValue<T>(string cacheKey, out T result)
        {
            IncrementRequestCount(cacheKey);
            CacheItem cachedItem = _cache.GetCacheItem(cacheKey);
            if (cachedItem == null)
            {
                result = default(T);
                return false;
            }
            result = (T)cachedItem.Value;
            return true;
        }

        protected void Add<T>(string key, T val, CacheItemPolicy cacheItemPolicy = null)
        {
            IncrementMissCount(key);
            _cache.Set(key, val, cacheItemPolicy ?? DefaultCachePolicy);
        }

        private void IncrementMissCount(string procedureName)
        {
            if (EnableHitCount)
            {
                HitCount.IncrementMiss(procedureName);
            }
        }

        private void IncrementRequestCount(string procedureName)
        {
            if (EnableHitCount)
            {
                HitCount.IncrementRequest(procedureName);
            }
        }

        protected string GenerateCacheKey(string primaryKey, params object[] parameters)
        {
            return string.Join(",", new object[] { primaryKey }.Concat(parameters).Select(o => o.ToString()));
        }
        

        #endregion
    }
}
