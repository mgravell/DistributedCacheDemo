## Problem statement:

> The `IDistributedCache` API is very minimal, requiring a lot of boilerplate code to check for values,
get missing values, serialize, deserialize, etc; this makes actually using the cache overly complex.

The purpose of this example is to show helper APIs for using `IDistributedCache`, so that callers
can reuse common code for the caching aspects, and focus on the things that impact *them*:

- what the key is (specified only once)
- where the data should come from if it isn't available
- what the expiration options should be

Helper extension methods are provided in `DistributedCacheExtensions` that allow sync and async
callbacks (to fetch missing data), optionally allowing the `TState` pattern (often associated
with static lambdas in high throughput scenarios - in many real-world scenarios, the `TState`
pattern can be overlooked with the simpler captured variables / non-static lambda approach.

The topic of serialization is discussed, with the example using `System.Text.Json` to avoid
requiring external dependencies; however, other serializers are available with a range of
different performance characteristics and feature sets.

Four routes are configured, using minimal APIs:

- `/sync` uses a synchronous simple lambda
- `/async` uses an asynchronous simple lambda
- `/{foo}/{bar}/sync` uses a synchronous static/`TState` lambda, also showing key-partitioning
- `/{foo}/{bar}/async` uses an asynchronous static/`TState` lambda, also showing key-partitioning

Each of these routes simply caches the current time, with a 30 second expiration; hitting the same
route in that interval should show the same time, before updating. It uses the in-memory cache
implementation, but [other distributed cache stores are available](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed).