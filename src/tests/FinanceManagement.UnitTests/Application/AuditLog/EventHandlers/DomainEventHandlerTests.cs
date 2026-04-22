using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.EventHandlers;
using FinanceManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Security.Claims;

namespace FinanceManagement.UnitTests.Application.AuditLog.EventHandlers
{
    public sealed class DomainEventHandlerTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoDbContext = new(MockBehavior.Strict);
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new(MockBehavior.Loose);
        private readonly Mock<IMongoCollection<AuditLogs>> _mockCollection = new(MockBehavior.Loose);

        private DomainEventHandler CreateSut() =>
            new(_mockMongoDbContext.Object, _mockHttpContextAccessor.Object);

        private AuditLogsDomainEvent CreateEvent(
            string actionDetail = "Create",
            string actionCode = "TEST_CREATE",
            string actionName = "TestCode001",
            string details = "Test record created",
            string module = "TestModule") =>
            new(actionDetail, actionCode, actionName, details, module);

        private void SetupMongoCollection()
        {
            _mockMongoDbContext
                .Setup(m => m.GetCollection<AuditLogs>("AuditLogs"))
                .Returns(_mockCollection.Object);
        }

        private void SetupHttpContext(int userId = 1, string userName = "test-user", string ip = "127.0.0.1")
        {
            var claims = new[]
            {
                new Claim("nameid", userId.ToString()),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", userName)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext
            {
                User = principal
            };
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ip);

            _mockHttpContextAccessor
                .Setup(a => a.HttpContext)
                .Returns(httpContext);
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidEvent_CallsInsertOneAsync()
        {
            SetupMongoCollection();
            SetupHttpContext();

            _mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<AuditLogs>(),
                    It.IsAny<InsertOneOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            var domainEvent = CreateEvent();

            await sut.Handle(domainEvent, CancellationToken.None);

            _mockCollection.Verify(
                c => c.InsertOneAsync(
                    It.IsAny<AuditLogs>(),
                    It.IsAny<InsertOneOptions?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidEvent_SetsActionFromNotification()
        {
            SetupMongoCollection();
            SetupHttpContext();

            AuditLogs? capturedLog = null;
            _mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<AuditLogs>(),
                    It.IsAny<InsertOneOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<AuditLogs, InsertOneOptions?, CancellationToken>((log, _, _) => capturedLog = log)
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await sut.Handle(CreateEvent(actionDetail: "Update", details: "Updated record", module: "Finance"), CancellationToken.None);

            capturedLog.Should().NotBeNull();
            capturedLog!.Action.Should().Be("Update");
            capturedLog.Details.Should().Be("Updated record");
            capturedLog.Module.Should().Be("Finance");
        }

        [Fact]
        public async Task Handle_ValidEvent_SetsCreatedByFromClaims()
        {
            SetupMongoCollection();
            SetupHttpContext(userId: 42, userName: "admin-user");

            AuditLogs? capturedLog = null;
            _mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<AuditLogs>(),
                    It.IsAny<InsertOneOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<AuditLogs, InsertOneOptions?, CancellationToken>((log, _, _) => capturedLog = log)
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await sut.Handle(CreateEvent(), CancellationToken.None);

            capturedLog.Should().NotBeNull();
            capturedLog!.CreatedBy.Should().Be(42);
            capturedLog.CreatedByName.Should().Be("admin-user");
        }

        [Fact]
        public async Task Handle_ValidEvent_SetsCreatedAtToUtcNow()
        {
            SetupMongoCollection();
            SetupHttpContext();

            AuditLogs? capturedLog = null;
            _mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<AuditLogs>(),
                    It.IsAny<InsertOneOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<AuditLogs, InsertOneOptions?, CancellationToken>((log, _, _) => capturedLog = log)
                .Returns(Task.CompletedTask);

            var before = DateTimeOffset.UtcNow;
            var sut = CreateSut();
            await sut.Handle(CreateEvent(), CancellationToken.None);
            var after = DateTimeOffset.UtcNow;

            capturedLog.Should().NotBeNull();
            capturedLog!.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        }

        [Fact]
        public async Task Handle_NullHttpContext_SetsNullableFieldsToNull()
        {
            SetupMongoCollection();
            _mockHttpContextAccessor
                .Setup(a => a.HttpContext)
                .Returns((HttpContext?)null);

            AuditLogs? capturedLog = null;
            _mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<AuditLogs>(),
                    It.IsAny<InsertOneOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<AuditLogs, InsertOneOptions?, CancellationToken>((log, _, _) => capturedLog = log)
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await sut.Handle(CreateEvent(), CancellationToken.None);

            capturedLog.Should().NotBeNull();
            capturedLog!.IPAddress.Should().BeNull();
            capturedLog.CreatedByName.Should().BeNull();
            capturedLog.CreatedBy.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ValidEvent_GetsCollectionByName()
        {
            SetupMongoCollection();
            SetupHttpContext();

            _mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<AuditLogs>(),
                    It.IsAny<InsertOneOptions?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await sut.Handle(CreateEvent(), CancellationToken.None);

            _mockMongoDbContext.Verify(
                m => m.GetCollection<AuditLogs>("AuditLogs"),
                Times.Once);
        }
    }
}
