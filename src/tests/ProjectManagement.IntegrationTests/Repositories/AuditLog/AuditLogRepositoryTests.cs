using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Common.Interfaces.AuditLog;
using ProjectManagement.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace ProjectManagement.IntegrationTests.Repositories.AuditLog
{
    /// <summary>
    /// Placeholder tests for AuditLogRepository (MongoDB-backed).
    /// These tests verify constructor contracts and argument validation only.
    /// Full integration tests require a running MongoDB instance and are
    /// therefore excluded from the standard SQL-based test suite.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class AuditLogRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AuditLogRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private static AuditLogRepository CreateRepo(
            IMongoDbContext? mongoCtx = null,
            IHttpContextAccessor? httpCtx = null)
        {
            mongoCtx ??= new Mock<IMongoDbContext>(MockBehavior.Loose).Object;
            httpCtx ??= new Mock<IHttpContextAccessor>(MockBehavior.Loose).Object;
            return new AuditLogRepository(mongoCtx, httpCtx);
        }

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_Should_Throw_ArgumentNullException_When_AuditLog_Is_Null()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.CreateAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("auditLog");
        }

        [Fact]
        public void Constructor_Should_Accept_Dependencies()
        {
            var mongoMock = new Mock<IMongoDbContext>(MockBehavior.Loose);
            var httpMock = new Mock<IHttpContextAccessor>(MockBehavior.Loose);

            var repo = new AuditLogRepository(mongoMock.Object, httpMock.Object);

            repo.Should().NotBeNull();
        }

        // --- GetByAuditLogNameAsync ---

        [Fact]
        public async Task GetByAuditLogNameAsync_Should_Throw_When_SearchPattern_IsNull()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetByAuditLogNameAsync(null!);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("searchPattern");
        }

        [Fact]
        public async Task GetByAuditLogNameAsync_Should_Throw_When_SearchPattern_IsEmpty()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetByAuditLogNameAsync("");

            await act.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("searchPattern");
        }

        [Fact]
        public async Task GetByAuditLogNameAsync_Should_Throw_When_SearchPattern_IsWhitespace()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetByAuditLogNameAsync("   ");

            await act.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("searchPattern");
        }
    }
}
