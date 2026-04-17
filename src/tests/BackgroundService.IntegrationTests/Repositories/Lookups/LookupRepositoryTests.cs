using BackgroundService.Infrastructure.Repositories.Common;
using BackgroundService.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace BackgroundService.IntegrationTests.Repositories.Lookups
{
    /// <summary>
    /// LookupRepository queries AppData.Department/Unit/Menus and AppSecurity.Users/UserSessions
    /// tables that live in the UserManagement schema and are NOT in the BackgroundService
    /// NotificationDbContext test schema. Tests here cover input-guard paths that short-circuit
    /// before any DB round-trip — full table-backed paths require a unified schema fixture.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class LookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private LookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString), new MemoryCache(new MemoryCacheOptions()));

        // --- Constructor guards ---

        [Fact]
        public void Ctor_ShouldThrow_WhenConnection_IsNull()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());

            var act = () => new LookupRepository(null!, cache);

            act.Should().Throw<ArgumentNullException>()
               .And.ParamName.Should().Be("dbConnection");
        }

        [Fact]
        public void Ctor_ShouldThrow_WhenCache_IsNull()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);

            var act = () => new LookupRepository(conn, null!);

            act.Should().Throw<ArgumentNullException>()
               .And.ParamName.Should().Be("memoryCache");
        }

        // --- GetMenuNamesAsync short-circuit guards ---

        [Fact]
        public async Task GetMenuNamesAsync_ReturnsEmpty_WhenIds_IsNull()
        {
            var result = await CreateRepo().GetMenuNamesAsync(null!, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMenuNamesAsync_ReturnsEmpty_WhenIds_IsEmpty()
        {
            var result = await CreateRepo().GetMenuNamesAsync(Array.Empty<int>(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMenuNamesAsync_ReturnsEmpty_WhenOnlyZeroIds()
        {
            var result = await CreateRepo().GetMenuNamesAsync(new[] { 0, 0, 0 }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        // --- GetUserNamesAsync short-circuit guards ---

        [Fact]
        public async Task GetUserNamesAsync_ReturnsEmpty_WhenIds_IsNull()
        {
            var result = await CreateRepo().GetUserNamesAsync(null!, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserNamesAsync_ReturnsEmpty_WhenIds_IsEmpty()
        {
            var result = await CreateRepo().GetUserNamesAsync(Array.Empty<int>(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserNamesAsync_ReturnsEmpty_WhenOnlyNonPositiveIds()
        {
            var result = await CreateRepo().GetUserNamesAsync(new[] { 0, -1 }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        // --- GetMenuIdByNameAsync short-circuit guards ---

        [Fact]
        public async Task GetMenuIdByNameAsync_ReturnsNull_WhenName_IsNull()
        {
            var result = await CreateRepo().GetMenuIdByNameAsync(null!, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetMenuIdByNameAsync_ReturnsNull_WhenName_IsWhitespace()
        {
            var result = await CreateRepo().GetMenuIdByNameAsync("   ", CancellationToken.None);

            result.Should().BeNull();
        }

        // --- GetSessionByJwtIdAsync short-circuit guards ---

        [Fact]
        public async Task GetSessionByJwtIdAsync_ReturnsNull_WhenJwtId_IsEmpty()
        {
            var result = await CreateRepo().GetSessionByJwtIdAsync("", CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSessionByJwtIdAsync_ReturnsNull_WhenJwtId_IsWhitespace()
        {
            var result = await CreateRepo().GetSessionByJwtIdAsync("   ", CancellationToken.None);

            result.Should().BeNull();
        }

        // --- UpdateSessionLastActivityAsync short-circuit guards ---

        [Fact]
        public async Task UpdateSessionLastActivity_ReturnsFalse_WhenJwtId_IsEmpty()
        {
            var result = await CreateRepo().UpdateSessionLastActivityAsync("", DateTimeOffset.UtcNow, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
