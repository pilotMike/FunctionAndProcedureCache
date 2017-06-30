# Function and ProcedureCache
Wrapper classes for caching funcitons and procedure calls by their arguments.  

# SQL Stored Procedure cache  
A cache wrapper for an ISQLSource object, which should handle the actual calling for ADO.  
Usage:  
```
var cache = new StoredProcedureCache(new ADOSqlSource()/* this will be your own implementation */);  
var proc = "MyStoredProcedure";  
object[] arguments = new object[] { "@input1", 1 };  
var result = cache.ExecuteStoredProcedure(proc, arguments);
```

  
# Function Cache  
A cache wrapper for caching functions and expressions by their arguments.  
  
Functions require their arguments to be set explicitly.  
```
Func<object, object> f = x => x;
var cache = new FunctionCache();
cache.Get(f, parameters: 1);  
```

However, using an expression can parse the arguments passed to a method call.  
```
cache.Get(() => myDataBaseContext.ExecuteProcedure(proc, arguments));
cache.Get(() => f(1));
```
  
# Metrics  
The caches' HitCount property contains cache hit and miss counts.
```
var cache = new FunctionCache() { EnableHitCount = true };
var totals = cache.HitCount.Totals;
HitCount procedureCalls = cache.HitCount[cacheKeyName];
```

# Extensibility  
The base class BasicMemoryCache provides basic access to the underlying .NET Default Memory Cache instance with protected methods for `` Get, TryGetValue,`` and ``Add``

#Changing the cache duration
The BasicMemoryCache exposes a CacheItemPolicy that can be overridden per instance or per method via the extending classes.  
```
var cache = new FunctionCache() { CacheItemPolicy = new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromDays(1) } } ;
cache.Get(() => 1);
cache.Get(() => 1, new CacheItemPolicy() /* ... */);
```
