using System.Collections.Generic;
using System.Linq;

namespace GlobalCache.Tracking
{
    public class HitCountTrack
    {
        protected Dictionary<string, CallCount> BackingStore;

        public HitCountTrack()
        {
            BackingStore = new Dictionary<string, CallCount>();
        }

        public HitCountTrack(Dictionary<string, CallCount> backingStore)
        {
            BackingStore = backingStore;
        }

        public void IncrementRequest(string cacheKey)
        {
            CallCount callCount;
            BackingStore.TryGetValue(cacheKey, out callCount);
            BackingStore[cacheKey] = callCount.IncrementRequest();
        }

        public void IncrementMiss(string cacheKey)
        {
            CallCount callCount;
            BackingStore.TryGetValue(cacheKey, out callCount);
            BackingStore[cacheKey] = callCount.IncrementExecution();
        }
        
        public CallCount this[string cacheKey]
        {
            get { return BackingStore[cacheKey]; }
            set { BackingStore[cacheKey] = value; }
        }
        
        public CallCount Totals => BackingStore.Values.Aggregate((a, b) => a + b);
    }
}
