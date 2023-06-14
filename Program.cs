using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapGet("/sync", async (IDistributedCache cache, CancellationToken abort) =>
{
    var timeObject = await cache.GetAsync("sync time cache key", () => {
        // perform some potentially expensive operation when the cache data is
        // unavailable; this could take to databases, http APIs, etc
        return new { Time = DateTime.UtcNow }; // this could be a complex object with multiple values
    }, CacheOptions.TenSeconds, abort);
    return $"The time reported (sync getter) is: {timeObject.Time}";
});
app.MapGet("/async", async (IDistributedCache cache, CancellationToken abort) =>
{
    var timeObject = await cache.GetAsync("async time cache key", async ct => {
        // perform some potentially expensive operation when the cache data is
        // unavailable; this could take to databases, http APIs, etc
        await Task.Delay(2000, ct);
        return new { Time = DateTime.UtcNow }; // this could be a complex object with multiple values
    }, CacheOptions.TenSeconds, abort);
    return $"The time reported (async getter) is: {timeObject.Time}";
});
app.MapGet("/{foo}/{bar}/sync", async (string foo, string bar, IDistributedCache cache, CancellationToken abort) =>
{
    // here we show the use of the "state" parameter; this *would* also work without "state" via "captured variables",
    // but by using the "state" parameter, we are able to use a static lambda (i.e. one that does not capture per-call
    // state, and therefore does not require per-call allocations)
    var timeObject = await cache.GetAsync($"{foo}:{bar} sync time cache key", // note we partition the cache by foo/bar via the key
        (Foo: foo, Bar: bar), // state values
        static state => {
        // perform some potentially expensive operation when the cache data is
        // unavailable; this could take to databases, http APIs, etc
        return new { Time = DateTime.UtcNow, state.Foo, state.Bar }; // this could be a complex object with multiple values
    }, CacheOptions.TenSeconds, abort);
    return $"The time reported (sync getter, {timeObject.Foo}/{timeObject.Bar}) is: {timeObject.Time}";
});
app.MapGet("/{foo}/{bar}/async", async (string foo, string bar, IDistributedCache cache, CancellationToken abort) =>
{
    // here we show the use of the "state" parameter; this *would* also work without "state" via "captured variables",
    // but by using the "state" parameter, we are able to use a static lambda (i.e. one that does not capture per-call
    // state, and therefore does not require per-call allocations)
    var timeObject = await cache.GetAsync($"{foo}:{bar} async time cache key", // note we partition the cache by foo/bar via the key
        (Foo: foo, Bar: bar), // state values
        static async (state, ct) => {
            // perform some potentially expensive operation when the cache data is
            // unavailable; this could take to databases, http APIs, etc
            await Task.Delay(2000, ct);
            return new { Time = DateTime.UtcNow, state.Foo, state.Bar }; // this could be a complex object with multiple values
        }, CacheOptions.TenSeconds, abort);
    return $"The time reported (async getter, {timeObject.Foo}/{timeObject.Bar}) is: {timeObject.Time}";
});

app.UseRouting();
app.UseAuthorization();
app.Run();

static class CacheOptions
{
    public static DistributedCacheEntryOptions TenSeconds { get; } = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10) };
}