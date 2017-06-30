using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;

namespace GlobalCache.Caching
{
    /// <summary>
    /// A cache for SQL stored procedures by parameters.
    /// Override the Default Cache Item Policy to change
    /// how long results are cached.
    /// </summary>
    public class StoredProcedureCache : BasicMemoryCache
    {
        #region Fields
        
        private ISQLSource _sqlSource;

        #endregion
        
        #region Constructor
        
        /// <summary>
        /// <seealso cref="APO.Business.Caching.ISQLSource"/>
        /// </summary>
        /// <param name="sqlSource">e.g. SQLSource.CIA</param>
        [DebuggerStepThrough]
        public StoredProcedureCache(ISQLSource sqlSource) 
        {
            _sqlSource = sqlSource;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes and returns a stored procedure, cached based on the procedure name and parameter inputs.
        /// </summary>
        /// <param name="procedureName">stored procedure to call</param>
        /// <param name="parameters">stored procedure parameters</param>
        [DebuggerStepThrough]
        public DataTable ExecuteStoredProcedure(string procedureName, params object[] parameters)
        {
            return ExecuteStoredProcedure(DefaultCachePolicy, procedureName, parameters);
        }

        [DebuggerStepThrough]
        /// <summary>
        /// Executes and returns a stored procedure, cached based on the procedure name and parameter inputs.
        /// </summary>
        /// <param name="cacheItemPolicy">Uses default if null</param>
        /// <param name="procedureName">stored procedure to call</param>
        /// <param name="parameters">stored procedure parameters</param>
        public DataTable ExecuteStoredProcedure(CacheItemPolicy cacheItemPolicy, string procedureName, params object[] parameters)
        {
            string cacheKey = GenerateCacheKey(procedureName, parameters);

            DataTable result;
            if (!TryGetValue(cacheKey, out result))
            {
                result = _sqlSource.GetSPDataTable(procedureName, parameters);
                Add(cacheKey, result, cacheItemPolicy);
            }

            return result;
        }

        [DebuggerStepThrough]
        /// <summary>
        /// Calls a stored procedure and retrieves a column from the result set.
        /// </summary>
        /// <typeparam name="T">data type of the column</typeparam>
        /// <param name="procedureName">stored procedure to call</param>
        /// <param name="columnName">column to retrieve</param>
        /// <param name="parameters">stored procedure parameters</param>
        public T ExecuteStoredProcedureField<T>(string procedureName, string columnName,
            params object[] parameters)
        {
            return ExecuteStoredProcedureField<T>(procedureName, columnName, DefaultCachePolicy, parameters);
        }

        [DebuggerStepThrough]
        /// <summary>
        /// Calls a stored procedure and retrieves a column from the result set.
        /// </summary>
        /// <typeparam name="T">data type of the column</typeparam>
        /// <param name="procedureName">stored procedure to call</param>
        /// <param name="columnName">column to retrieve</param>
        /// <param name="parameters">stored procedure parameters</param>
        public T ExecuteStoredProcedureField<T>(string procedureName, string columnName,
            CacheItemPolicy cacheItemPolicy, params object[] parameters)
        {
            var cacheKey = GenerateCacheKey(procedureName, new object[] { columnName }.Concat(parameters).ToArray());
            T result;
            if (!TryGetValue(cacheKey, out result))
            {
                var table = _sqlSource.GetSPDataTable(procedureName, parameters);
                result = table.Rows.Cast<DataRow>().DefaultIfEmpty(table.NewRow())
                    .First().Field<T>(columnName);
                Add(cacheKey, result);
            }
            return result;
        }

        [DebuggerStepThrough]
        /// <summary>
        /// Returns a scalar properties from a stored procedure.
        /// </summary>
        /// <typeparam name="T">column return type</typeparam>
        /// <param name="procedureName">procedure to call</param>
        /// <param name="parameters">procedure parameters</param>
        public T ExecuteScalar<T>(string procedureName, params object[] parameters)
        {
            return ExecuteScalar<T>(procedureName, DefaultCachePolicy, parameters);
        }

        [DebuggerStepThrough]
        /// <summary>
        /// Returns a scalar properties from a stored procedure.
        /// </summary>
        /// <typeparam name="T">column return type</typeparam>
        /// <param name="procedureName">procedure to call</param>
        /// <param name="parameters">procedure parameters</param>
        /// <param name="cacheItemPolicy">custom cache policy</param>
        public T ExecuteScalar<T>(string procedureName, CacheItemPolicy cacheItemPolicy, params object[] parameters)
        {
            var cacheKey = GenerateCacheKey(procedureName, parameters);
            var result = base.Get(cacheKey, () => (T)_sqlSource.GetSPField(procedureName, parameters));
            return result;
        }

        #endregion

    }
    
}
