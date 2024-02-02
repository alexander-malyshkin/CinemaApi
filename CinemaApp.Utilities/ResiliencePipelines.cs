using System.Diagnostics;
using Polly;
using Polly.CircuitBreaker;
using Polly.Hedging;

namespace CinemaApp.Utilities
{
    public static class ResiliencePipelines
    {
        public const string HedgingRetryWithCircuitBreaker = "HedgingRetryWithCircuitBreaker";

        public static void AddHedgingRetryWithCircuitBreakerPipeline<T>(ResiliencePipelineBuilder<T> pipelineBuilder)
        {
            pipelineBuilder.AddHedging(new HedgingStrategyOptions<T>
            {
                ShouldHandle = new PredicateBuilder<T>().Handle<Exception>(),
                MaxHedgedAttempts = 2, // Issue the original request and 2 more hedged requests in parallel
                Delay = TimeSpan.FromMilliseconds(500) // Tail mode
            });

            AddRetryAndCircuitBreaker(pipelineBuilder);
        }
        public static void AddRetryAndCircuitBreaker<T>(ResiliencePipelineBuilder<T> pipelineBuilder)
        {
            pipelineBuilder.AddRetry(new ()
            {
                ShouldHandle = new PredicateBuilder<T>().Handle<Exception>(ex => ex is not BrokenCircuitException),
                MaxRetryAttempts = int.MaxValue,
                BackoffType = DelayBackoffType.Linear,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(200) // this value could be configurable
            });

            // the values could be configurable
            pipelineBuilder.AddCircuitBreaker(new ()
            {
                ShouldHandle = new PredicateBuilder<T>().Handle<Exception>(),
                FailureRatio = 0.9, 
                SamplingDuration = TimeSpan.FromSeconds(2),
                MinimumThroughput = 4,
                BreakDuration = TimeSpan.FromSeconds(3)
            });
        }
    }
}
