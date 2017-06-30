using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using Telerik.JustMock;
using GlobalCache.Caching;

namespace GlobalCache.Test.Caching
{
    [TestClass]
    public class MemCacheTests
    {
        private StoredProcedureCache _cache;

        [TestInitialize]
        public void Setup()
        {
            var src = Mock.Create<ISQLSource>();
            Mock.Arrange(src, s => s.GetSPDataTable(Arg.AnyString, Arg.IsAny<object[]>())).Returns(new DataTable());
            _cache = new StoredProcedureCache(src);
            _cache.EnableHitCount = true;
        }

        [TestMethod]
        public void cache_hit_for_stored_procedure_keyed_by_parameters()
        {
            var proc = "simulated_stored_proc";
            object[] parameters = new object[] {
                    "@input1", 0,
                    "@input2", 1,
                    "@input3", 2,
                    "@input4", 3,
                    "@input5", "",
                    "@input6", string.Empty,
                    "@input7", "",
                    "@input8", string.Empty,
                    "@input9", 0,
                    "@input10", 1};

            for (int i = 0; i < 5; i++)
            {
                _cache.ExecuteStoredProcedure(proc, parameters);
            }
            
            Assert.AreEqual(1, _cache.HitCount.Totals.ExecutionCount);
            Assert.AreEqual(5, _cache.HitCount.Totals.RequestCount);
        }

        [TestMethod]
        public void correctly_misses_cache()
        {
            var proc = "a";

            for (int i = 0; i < 5; i++)
            {
                _cache.ExecuteStoredProcedure(proc, i);
            }

            Assert.AreEqual(5, _cache.HitCount.Totals.ExecutionCount);
        }
    }
}
