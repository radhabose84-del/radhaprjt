using GateEntryManagement.Application.Common.Interfaces;
using GateEntryManagement.Domain.Entities;
using GateEntryManagement.Infrastructure.Repositories.AuditLog;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace GateEntryManagement.IntegrationTests.Repositories.AuditLog
{
    /// <summary>
    /// AuditLogRepository wraps IMongoDbContext. Tests verify repo interactions against
    /// the abstraction without requiring a real MongoDB instance.
    /// </summary>
    public sealed class AuditLogMongoRepositoryTests
    {
        private readonly Mock<IMongoDbContext> _mockMongo = new(MockBehavior.Strict);
        private readonly Mock<IHttpContextAccessor> _mockHttp = new(MockBehavior.Loose);

        private AuditLogRepository CreateRepo() =>
            new(_mockMongo.Object, _mockHttp.Object);

        [Fact]
        public async Task CreateAsync_Should_Insert_Into_AuditLogs_Collection()
        {
            var collection = new Mock<IMongoCollection<dynamic>>(MockBehavior.Loose);
            _mockMongo.Setup(c => c.GetCollection<dynamic>("AuditLogs")).Returns(collection.Object);

            var auditLog = new AuditLogs
            {
                Action = "Create",
                Details = "Test",
                Module = "GateEntry",
                CreatedByName = "tester"
            };

            var result = await CreateRepo().CreateAsync(auditLog);

            result.Should().BeSameAs(auditLog);
            collection.Verify(c => c.InsertOneAsync(
                It.IsAny<object>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_AuditLog_Is_Null()
        {
            Func<Task> act = async () => await CreateRepo().CreateAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetByAuditLogNameAsync_Should_Throw_When_SearchPattern_Empty()
        {
            Func<Task> act = async () => await CreateRepo().GetByAuditLogNameAsync("");
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetByAuditLogNameAsync_Should_Throw_When_SearchPattern_Whitespace()
        {
            Func<Task> act = async () => await CreateRepo().GetByAuditLogNameAsync("   ");
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}
