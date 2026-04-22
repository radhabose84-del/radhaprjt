using GateEntryManagement.Application.Common.Interfaces;
using GateEntryManagement.Application.EventHandlers;
using GateEntryManagement.Domain.Entities;
using GateEntryManagement.Domain.Events;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Security.Claims;

namespace GateEntryManagement.UnitTests.Application.EventHandlers
{
    public sealed class DomainEventHandlerTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoDbContext = new(MockBehavior.Strict);
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new(MockBehavior.Loose);

        private DomainEventHandler CreateSut() =>
            new(_mockMongoDbContext.Object, _mockHttpContextAccessor.Object);

        private AuditLogsDomainEvent CreateNotification(
            string actionDetail = "Create",
            string actionCode = "MISC_MASTER_CREATE",
            string actionName = "MM001",
            string details = "Record created",
            string module = "MiscMaster") =>
            new(actionDetail, actionCode, actionName, details, module);

        private void SetupMongoCollection()
        {
            var mockCollection = new Mock<IMongoCollection<AuditLogs>>(MockBehavior.Loose);
            _mockMongoDbContext
                .Setup(m => m.GetCollection<AuditLogs>("AuditLogs"))
                .Returns(mockCollection.Object);
        }

        private void SetupHttpContextWithClaims(int userId = 1, string userName = "testuser")
        {
            var claims = new List<Claim>
            {
                new Claim("UserId", userId.ToString()),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", userName)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = principal };
            _mockHttpContextAccessor
                .Setup(a => a.HttpContext)
                .Returns(httpContext);
        }

        [Fact]
        public async Task Handle_ValidNotification_CallsInsertOneAsync()
        {
            // Arrange
            SetupHttpContextWithClaims();
            var mockCollection = new Mock<IMongoCollection<AuditLogs>>(MockBehavior.Loose);
            _mockMongoDbContext
                .Setup(m => m.GetCollection<AuditLogs>("AuditLogs"))
                .Returns(mockCollection.Object);

            var notification = CreateNotification();
            var sut = CreateSut();

            // Act
            await sut.Handle(notification, CancellationToken.None);

            // Assert
            mockCollection.Verify(
                c => c.InsertOneAsync(
                    It.IsAny<AuditLogs>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_SetsActionFromNotification()
        {
            // Arrange
            SetupHttpContextWithClaims();
            AuditLogs? capturedLog = null;
            var mockCollection = new Mock<IMongoCollection<AuditLogs>>(MockBehavior.Loose);
            mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<AuditLogs>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<AuditLogs, InsertOneOptions?, CancellationToken>((log, _, _) => capturedLog = log);
            _mockMongoDbContext
                .Setup(m => m.GetCollection<AuditLogs>("AuditLogs"))
                .Returns(mockCollection.Object);

            var notification = CreateNotification(actionDetail: "Update", module: "GateEntry");
            var sut = CreateSut();

            // Act
            await sut.Handle(notification, CancellationToken.None);

            // Assert
            capturedLog.Should().NotBeNull();
            capturedLog!.Action.Should().Be("Update");
            capturedLog.Module.Should().Be("GateEntry");
        }

        [Fact]
        public async Task Handle_ValidNotification_SetsDetailsFromNotification()
        {
            // Arrange
            SetupHttpContextWithClaims();
            AuditLogs? capturedLog = null;
            var mockCollection = new Mock<IMongoCollection<AuditLogs>>(MockBehavior.Loose);
            mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<AuditLogs>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<AuditLogs, InsertOneOptions?, CancellationToken>((log, _, _) => capturedLog = log);
            _mockMongoDbContext
                .Setup(m => m.GetCollection<AuditLogs>("AuditLogs"))
                .Returns(mockCollection.Object);

            var notification = CreateNotification(details: "MiscMaster record created with Id 42");
            var sut = CreateSut();

            // Act
            await sut.Handle(notification, CancellationToken.None);

            // Assert
            capturedLog.Should().NotBeNull();
            capturedLog!.Details.Should().Be("MiscMaster record created with Id 42");
        }

        [Fact]
        public async Task Handle_WithUserClaims_SetsCreatedByFromClaims()
        {
            // Arrange
            SetupHttpContextWithClaims(userId: 5, userName: "admin");
            AuditLogs? capturedLog = null;
            var mockCollection = new Mock<IMongoCollection<AuditLogs>>(MockBehavior.Loose);
            mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<AuditLogs>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<AuditLogs, InsertOneOptions?, CancellationToken>((log, _, _) => capturedLog = log);
            _mockMongoDbContext
                .Setup(m => m.GetCollection<AuditLogs>("AuditLogs"))
                .Returns(mockCollection.Object);

            var sut = CreateSut();

            // Act
            await sut.Handle(CreateNotification(), CancellationToken.None);

            // Assert
            capturedLog.Should().NotBeNull();
            capturedLog!.CreatedBy.Should().Be(5);
            capturedLog.CreatedByName.Should().Be("admin");
        }

        [Fact]
        public async Task Handle_NullHttpContext_SetsCreatedByToZero()
        {
            // Arrange
            _mockHttpContextAccessor
                .Setup(a => a.HttpContext)
                .Returns((HttpContext?)null);

            AuditLogs? capturedLog = null;
            var mockCollection = new Mock<IMongoCollection<AuditLogs>>(MockBehavior.Loose);
            mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<AuditLogs>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<AuditLogs, InsertOneOptions?, CancellationToken>((log, _, _) => capturedLog = log);
            _mockMongoDbContext
                .Setup(m => m.GetCollection<AuditLogs>("AuditLogs"))
                .Returns(mockCollection.Object);

            var sut = CreateSut();

            // Act
            await sut.Handle(CreateNotification(), CancellationToken.None);

            // Assert
            capturedLog.Should().NotBeNull();
            capturedLog!.CreatedBy.Should().Be(0);
            capturedLog.CreatedByName.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ValidNotification_GetsAuditLogsCollection()
        {
            // Arrange
            SetupHttpContextWithClaims();
            SetupMongoCollection();
            var sut = CreateSut();

            // Act
            await sut.Handle(CreateNotification(), CancellationToken.None);

            // Assert
            _mockMongoDbContext.Verify(
                m => m.GetCollection<AuditLogs>("AuditLogs"),
                Times.Once);
        }
    }
}
