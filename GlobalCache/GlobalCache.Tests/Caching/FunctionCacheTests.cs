using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GlobalCache.Caching;
using System.Runtime.Caching;

namespace GlobalCache.Test.Caching
{
    [TestClass]
    public class FunctionCacheTests
    {
        [TestInitialize]
        public void Setup()
        {
            CreateCache();
        }


        [TestMethod]
        public void caches_function_by_parameters()
        {
            FunctionCache cache = CreateCache();

            for (int i = 0; i < 5; i++)
            {
                cache.Get(() => i, 0);
            }

            Assert.AreEqual(1, cache.HitCount.Totals.ExecutionCount);
        }

        [TestMethod]
        public void expression_cache_misses_for_different_inputs()
        {
            var cache = CreateCache();
            for (int i = 0; i < 5; i++)
            {
                cache.Get(() => i);
            }

            Assert.AreEqual(5, cache.HitCount.Totals.ExecutionCount);
        }

        [TestMethod]
        public void expression_cache_hits_for_same_inputs()
        {
            var cache = CreateCache();
            for (int i = 0; i < 5; i++)
            {
                cache.Get(() => 0);
            }

            Assert.AreEqual(1, cache.HitCount.Totals.ExecutionCount);
        }

        [TestMethod]
        public void expression_cache_can_call_another_method_and_use_the_result_value_misses()
        {
            var cache = CreateCache();
            Func<int, int> f = i => i;
            for (int i = 0; i < 5; i++)
            {
                cache.Get(() => f(i));
            }

            Assert.AreEqual(5, cache.HitCount.Totals.ExecutionCount);
        }

        [TestMethod]
        public void expression_cache_can_call_another_method_and_use_the_result_value_hits()
        {
            var cache = CreateCache();
            Func<int, int> f = i => 0;
            for (int i = 0; i < 5; i++)
            {
                cache.Get(() => f(i));
            }

            Assert.AreEqual(1, cache.HitCount.Totals.ExecutionCount);
        }

        int testCount = 0;
        private FunctionCache CreateCache()
        {
            return new FunctionCache(new MemoryCache((++testCount).ToString())) { EnableHitCount = true };
        }
    }
}
