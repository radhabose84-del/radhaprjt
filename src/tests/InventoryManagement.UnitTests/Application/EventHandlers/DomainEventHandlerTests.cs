using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.EventHandlers;
using InventoryManagement.Domain.Events;
using MongoDB.Driver;

namespace InventoryManagement.UnitTests.Application.EventHandlers
{
    public sealed class DomainEventHandlerTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoDbContext = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Strict);
        private readonly Mock<ITimeZoneService> _mockTimeZoneService = new(MockBehavior.Strict);

        private DomainEventHandler CreateSut() =>
            new(_mockMongoDbContext.Object, _mockIpService.Object, _mockTimeZoneService.Object);

        private readonly Mock<IMongoCollection<dynamic>> _mockCollection = new(MockBehavior.Loose);

        private void SetupDefaults()
        {
            _mockIpService.Setup(s => s.GetUserId()).Returns(1);
            _mockIpService.Setup(s => s.GetUserOS()).Returns("Windows 11");
            _mockIpService.Setup(s => s.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockIpService.Setup(s => s.GetUserAgent()).Returns("Chrome/120");
            _mockIpService.Setup(s => s.GetUserName()).Returns("test-user");

            _mockTimeZoneService.Setup(s => s.GetSystemTimeZone()).Returns("India Standard Time");
            _mockTimeZoneService.Setup(s => s.GetCurrentTime("India Standard Time"))
                .Returns(DateTimeOffset.UtcNow);

            _mockMongoDbContext.Setup(m => m.GetCollection<dynamic>("AuditLogs"))
                .Returns(_mockCollection.Object);
        }

        private static AuditLogsDomainEvent CreateNotification(
            string actionDetail = "Create",
            string actionCode = "TEST_CREATE",
            string actionName = "TestCode",
            string details = "Test entity created.",
            string module = "TestModule") =>
            new(actionDetail, actionCode, actionName, details, module);

        [Fact]
        public async Task Handle_ValidNotification_DoesNotThrow()
        {
            SetupDefaults();

            var notification = CreateNotification();

            var act = async () => await CreateSut().Handle(notification, CancellationToken.None);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Handle_ValidNotification_ReadsUserId()
        {
            SetupDefaults();

            await CreateSut().Handle(CreateNotification(), CancellationToken.None);

            _mockIpService.Verify(s => s.GetUserId(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_ReadsUserName()
        {
            SetupDefaults();

            await CreateSut().Handle(CreateNotification(), CancellationToken.None);

            _mockIpService.Verify(s => s.GetUserName(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_ReadsUserOS()
        {
            SetupDefaults();

            await CreateSut().Handle(CreateNotification(), CancellationToken.None);

            _mockIpService.Verify(s => s.GetUserOS(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_ReadsSystemIPAddress()
        {
            SetupDefaults();

            await CreateSut().Handle(CreateNotification(), CancellationToken.None);

            _mockIpService.Verify(s => s.GetSystemIPAddress(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_ReadsUserAgent()
        {
            SetupDefaults();

            await CreateSut().Handle(CreateNotification(), CancellationToken.None);

            _mockIpService.Verify(s => s.GetUserAgent(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_ReadsTimeZone()
        {
            SetupDefaults();

            await CreateSut().Handle(CreateNotification(), CancellationToken.None);

            _mockTimeZoneService.Verify(s => s.GetSystemTimeZone(), Times.Once);
            _mockTimeZoneService.Verify(
                s => s.GetCurrentTime("India Standard Time"),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_GetsAuditLogsCollection()
        {
            SetupDefaults();

            await CreateSut().Handle(CreateNotification(), CancellationToken.None);

            _mockMongoDbContext.Verify(
                c => c.GetCollection<dynamic>("AuditLogs"),
                Times.Once);
        }
    }
}
