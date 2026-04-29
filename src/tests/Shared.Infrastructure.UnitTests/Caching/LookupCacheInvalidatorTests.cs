using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Shared.Infrastructure.Caching;

namespace Shared.Infrastructure.UnitTests.Caching;

public sealed class LookupCacheInvalidatorTests
{
    private static LookupCacheInvalidator CreateSut() =>
        new(NullLogger<LookupCacheInvalidator>.Instance);

    private static IMemoryCache CreateMemoryCache() =>
        new MemoryCache(new MemoryCacheOptions { SizeLimit = 1000 });

    private static void SetWithToken(IMemoryCache cache, string key, object value, CancellationToken token)
    {
        var options = new MemoryCacheEntryOptions
        {
            Size = 1,
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };
        options.AddExpirationToken(new CancellationChangeToken(token));
        cache.Set(key, value, options);
    }

    [Fact]
    public void GetEvictionToken_SameLookup_ReturnsSameTokenUntilEvicted()
    {
        var sut = CreateSut();

        var t1 = sut.GetEvictionToken("IItemLookup");
        var t2 = sut.GetEvictionToken("IItemLookup");

        t1.Should().Be(t2);
    }

    [Fact]
    public void GetEvictionToken_DifferentLookups_ReturnsDifferentTokens()
    {
        var sut = CreateSut();

        var itemToken = sut.GetEvictionToken("IItemLookup");
        var customerToken = sut.GetEvictionToken("ICustomerLookup");

        itemToken.Should().NotBe(customerToken);
    }

    [Fact]
    public void Evict_RemovesAllCachedEntriesForLookup()
    {
        var sut = CreateSut();
        var cache = CreateMemoryCache();

        var token = sut.GetEvictionToken("IItemLookup");
        SetWithToken(cache, "IItemLookup:GetAll:NoArgs", new List<int> { 1, 2 }, token);
        SetWithToken(cache, "IItemLookup:GetById:5", "Item5", token);

        cache.TryGetValue("IItemLookup:GetAll:NoArgs", out _).Should().BeTrue();
        cache.TryGetValue("IItemLookup:GetById:5", out _).Should().BeTrue();

        sut.Evict("IItemLookup");

        cache.TryGetValue("IItemLookup:GetAll:NoArgs", out _).Should().BeFalse();
        cache.TryGetValue("IItemLookup:GetById:5", out _).Should().BeFalse();
    }

    [Fact]
    public void Evict_DoesNotRemoveEntriesForOtherLookups()
    {
        var sut = CreateSut();
        var cache = CreateMemoryCache();

        var itemToken = sut.GetEvictionToken("IItemLookup");
        var customerToken = sut.GetEvictionToken("ICustomerLookup");

        SetWithToken(cache, "IItemLookup:Key", "ItemData", itemToken);
        SetWithToken(cache, "ICustomerLookup:Key", "CustomerData", customerToken);

        sut.Evict("IItemLookup");

        cache.TryGetValue("IItemLookup:Key", out _).Should().BeFalse();
        cache.TryGetValue("ICustomerLookup:Key", out var customerValue).Should().BeTrue();
        customerValue.Should().Be("CustomerData");
    }

    [Fact]
    public void Evict_OnUnregisteredName_DoesNotThrow()
    {
        var sut = CreateSut();

        var act = () => sut.Evict("INeverRegisteredLookup");

        act.Should().NotThrow();
    }

    [Fact]
    public void Evict_NullName_DoesNotThrow()
    {
        var sut = CreateSut();

        var act = () => sut.Evict(null!);

        act.Should().NotThrow();
    }

    [Fact]
    public void Evict_EmptyName_DoesNotThrow()
    {
        var sut = CreateSut();

        var act = () => sut.Evict(string.Empty);

        act.Should().NotThrow();
    }

    [Fact]
    public void Evict_AfterEviction_NewEntriesCanBeCached()
    {
        var sut = CreateSut();
        var cache = CreateMemoryCache();

        var token1 = sut.GetEvictionToken("IItemLookup");
        SetWithToken(cache, "IItemLookup:Key1", "Value1", token1);

        sut.Evict("IItemLookup");

        var token2 = sut.GetEvictionToken("IItemLookup");
        SetWithToken(cache, "IItemLookup:Key2", "Value2", token2);

        token2.Should().NotBe(token1);
        cache.TryGetValue("IItemLookup:Key1", out _).Should().BeFalse();
        cache.TryGetValue("IItemLookup:Key2", out var v).Should().BeTrue();
        v.Should().Be("Value2");
    }

    [Fact]
    public void Evict_ConcurrentCalls_DoNotThrow()
    {
        var sut = CreateSut();
        sut.GetEvictionToken("IItemLookup");

        var act = () => Parallel.For(0, 100, _ => sut.Evict("IItemLookup"));

        act.Should().NotThrow();
    }

    [Fact]
    public void GetEvictionToken_AfterEvict_ReturnsFreshToken()
    {
        var sut = CreateSut();

        var oldToken = sut.GetEvictionToken("IItemLookup");
        sut.Evict("IItemLookup");
        var newToken = sut.GetEvictionToken("IItemLookup");

        newToken.Should().NotBe(oldToken);
        oldToken.IsCancellationRequested.Should().BeTrue();
        newToken.IsCancellationRequested.Should().BeFalse();
    }
}
