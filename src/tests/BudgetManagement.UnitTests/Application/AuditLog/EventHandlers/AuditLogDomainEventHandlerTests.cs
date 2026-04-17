using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.EventHandlers;
using BudgetManagement.Domain.Events;
using Contracts.Interfaces;
using MongoDB.Driver;

namespace BudgetManagement.UnitTests.Application.AuditLog.EventHandlers
{
    public sealed class AuditLogDomainEventHandlerTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoDbContext = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Strict);
        private readonly Mock<ITimeZoneService> _mockTimeZoneService = new(MockBehavior.Strict);
        private readonly Mock<IMongoCollection<dynamic>> _mockCollection = new(MockBehavior.Loose);

        private DomainEventHandler CreateSut() =>
            new(_mockMongoDbContext.Object, _mockIpService.Object, _mockTimeZoneService.Object);

        private void SetupHappyPath()
        {
            _mockMongoDbContext
                .Setup(m => m.GetCollection<dynamic>("AuditLogs"))
                .Returns(_mockCollection.Object);

            _mockIpService.Setup(s => s.GetUserId()).Returns(1);
            _mockIpService.Setup(s => s.GetUserOS()).Returns("Windows 11");
            _mockIpService.Setup(s => s.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockIpService.Setup(s => s.GetUserAgent()).Returns("Chrome");
            _mockIpService.Setup(s => s.GetUserName()).Returns("test-user");

            _mockTimeZoneService.Setup(s => s.GetSystemTimeZone()).Returns("India Standard Time");
            _mockTimeZoneService.Setup(s => s.GetCurrentTime("India Standard Time"))
                .Returns(DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Handle_ValidEvent_CallsInsertOneAsync()
        {
            SetupHappyPath();
            var sut = CreateSut();
            var notification = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "BG_CREATE",
                actionName: "BG001",
                details: "BudgetGroup created.",
                module: "BudgetGroup");

            await sut.Handle(notification, CancellationToken.None);

            _mockCollection.Verify(
                c => c.InsertOneAsync(
                    It.IsAny<object>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidEvent_GetsAuditLogsCollection()
        {
            SetupHappyPath();
            var sut = CreateSut();
            var notification = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "BG_UPDATE",
                actionName: "1",
                details: "Updated.",
                module: "BudgetGroup");

            await sut.Handle(notification, CancellationToken.None);

            _mockMongoDbContext.Verify(
                m => m.GetCollection<dynamic>("AuditLogs"),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidEvent_ReadsUserInfoFromIpService()
        {
            SetupHappyPath();
            var sut = CreateSut();
            var notification = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "BG_DELETE",
                actionName: "1",
                details: "Deleted.",
                module: "BudgetGroup");

            await sut.Handle(notification, CancellationToken.None);

            _mockIpService.Verify(s => s.GetUserId(), Times.Once);
            _mockIpService.Verify(s => s.GetUserName(), Times.Once);
            _mockIpService.Verify(s => s.GetUserOS(), Times.Once);
            _mockIpService.Verify(s => s.GetSystemIPAddress(), Times.Once);
            _mockIpService.Verify(s => s.GetUserAgent(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidEvent_ReadsTimeZone()
        {
            SetupHappyPath();
            var sut = CreateSut();
            var notification = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "MM_CREATE",
                actionName: "MM001",
                details: "MiscMaster created.",
                module: "MiscMaster");

            await sut.Handle(notification, CancellationToken.None);

            _mockTimeZoneService.Verify(s => s.GetSystemTimeZone(), Times.Once);
            _mockTimeZoneService.Verify(
                s => s.GetCurrentTime("India Standard Time"),
                Times.Once);
        }
    }
}
