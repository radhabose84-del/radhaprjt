using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Moq;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Infrastructure.Repositories;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.AuditLog
{
    /// <summary>
    /// AuditLogMongoRepository wraps IMongoDbContext — tests verify repository interacts
    /// with the collection through the abstraction. A real MongoDB instance is not required.
    /// </summary>
    public sealed class AuditLogMongoRepositoryTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoContext = new(MockBehavior.Strict);
        private readonly Mock<IHttpContextAccessor> _mockHttpAccessor = new(MockBehavior.Loose);

        private AuditLogRepository CreateRepo() =>
            new(_mockMongoContext.Object, _mockHttpAccessor.Object);

        [Fact]
        public async Task CreateAsync_Should_Insert_Into_AuditLogs_Collection()
        {
            var collection = new Mock<IMongoCollection<dynamic>>(MockBehavior.Loose);
            _mockMongoContext
                .Setup(c => c.GetCollection<dynamic>("AuditLogs"))
                .Returns(collection.Object);

            var auditLog = new AuditLogs
            {
                Action = "Create",
                Details = "Test",
                Module = "TestModule",
                CreatedByName = "tester"
            };

            var result = await CreateRepo().CreateAsync(auditLog);

            result.Should().BeSameAs(auditLog);
            collection.Verify(
                c => c.InsertOneAsync(It.IsAny<object>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()),
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
