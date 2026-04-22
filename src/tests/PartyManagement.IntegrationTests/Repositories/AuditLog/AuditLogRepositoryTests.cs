using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.AuditLog;
using PartyManagement.Domain.Entities;
using PartyManagement.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;

namespace PartyManagement.IntegrationTests.Repositories.AuditLog
{
    /// <summary>
    /// Integration tests for AuditLogRepository (MongoDB-backed).
    /// These tests verify construction and argument validation only.
    /// Full MongoDB integration tests require a running MongoDB instance
    /// and cannot mock IAsyncCursor due to MongoDB driver version conflicts.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class AuditLogRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AuditLogRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void AuditLogRepository_CanBeConstructed()
        {
            var mockMongoContext = new Mock<IMongoDbContext>(MockBehavior.Loose);
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Loose);

            var repo = new AuditLogRepository(mockMongoContext.Object, mockHttpContextAccessor.Object);

            repo.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_NullAuditLog_ThrowsArgumentNullException()
        {
            var mockMongoContext = new Mock<IMongoDbContext>(MockBehavior.Loose);
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Loose);

            var repo = new AuditLogRepository(mockMongoContext.Object, mockHttpContextAccessor.Object);

            Func<Task> act = async () => await repo.CreateAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetByAuditLogNameAsync_EmptyPattern_ThrowsArgumentException()
        {
            var mockMongoContext = new Mock<IMongoDbContext>(MockBehavior.Loose);
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Loose);

            var repo = new AuditLogRepository(mockMongoContext.Object, mockHttpContextAccessor.Object);

            Func<Task> act = async () => await repo.GetByAuditLogNameAsync(string.Empty);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetByAuditLogNameAsync_NullPattern_ThrowsArgumentException()
        {
            var mockMongoContext = new Mock<IMongoDbContext>(MockBehavior.Loose);
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Loose);

            var repo = new AuditLogRepository(mockMongoContext.Object, mockHttpContextAccessor.Object);

            Func<Task> act = async () => await repo.GetByAuditLogNameAsync(null!);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public void IAuditLogRepository_Interface_HasExpectedMethods()
        {
            var type = typeof(IAuditLogRepository);

            type.GetMethod("CreateAsync").Should().NotBeNull();
            type.GetMethod("GetAllAsync").Should().NotBeNull();
            type.GetMethod("GetByAuditLogNameAsync").Should().NotBeNull();
        }
    }
}
