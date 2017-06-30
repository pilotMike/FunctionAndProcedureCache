namespace GlobalCache.Tracking
{
    public struct CallCount
    {
        /// <summary>
        /// Represents the number of times the procedure has been called.
        /// </summary>
        public int ExecutionCount { get; private set; }
        /// <summary>
        /// The number of times a request was made to the cache provider.
        /// </summary>
        public int RequestCount { get; private set; }

        internal CallCount IncrementExecution() => new CallCount { ExecutionCount = this.ExecutionCount + 1, RequestCount = RequestCount };
        internal CallCount IncrementRequest() => new CallCount { ExecutionCount = this.ExecutionCount, RequestCount = RequestCount + 1 };

        public static CallCount operator +(CallCount a, CallCount b) => 
            new CallCount
            {
                ExecutionCount = a.ExecutionCount + b.ExecutionCount,
                RequestCount = a.RequestCount + b.RequestCount
            };
    }
}
