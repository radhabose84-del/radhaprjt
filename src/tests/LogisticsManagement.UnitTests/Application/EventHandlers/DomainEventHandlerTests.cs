using Contracts.Interfaces;
using LogisticsManagement.Application.Common.Interfaces;
using LogisticsManagement.Application.EventHandlers;
using LogisticsManagement.Domain.Events;
using MongoDB.Driver;

namespace LogisticsManagement.UnitTests.Application.EventHandlers
{
    public sealed class DomainEventHandlerTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoDbContext = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpAddressService = new(MockBehavior.Strict);
        private readonly Mock<ITimeZoneService> _mockTimeZoneService = new(MockBehavior.Strict);

        private DomainEventHandler CreateSut() =>
            new(_mockMongoDbContext.Object, _mockIpAddressService.Object, _mockTimeZoneService.Object);

        private void SetupDefaultMocks()
        {
            _mockIpAddressService.Setup(s => s.GetUserId()).Returns(1);
            _mockIpAddressService.Setup(s => s.GetUserOS()).Returns("Windows 11");
            _mockIpAddressService.Setup(s => s.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockIpAddressService.Setup(s => s.GetUserAgent()).Returns("Chrome/120.0");
            _mockIpAddressService.Setup(s => s.GetUserName()).Returns("test-user");

            _mockTimeZoneService.Setup(s => s.GetSystemTimeZone()).Returns("India Standard Time");
            _mockTimeZoneService.Setup(s => s.GetCurrentTime("India Standard Time"))
                .Returns(DateTimeOffset.UtcNow);

            var mockCollection = new Mock<IMongoCollection<dynamic>>(MockBehavior.Loose);
            _mockMongoDbContext
                .Setup(c => c.GetCollection<dynamic>("AuditLogs"))
                .Returns(mockCollection.Object);
        }

        private static AuditLogsDomainEvent CreateTestEvent(
            string actionDetail = "Create",
            string actionCode = "FREIGHT_MASTER_CREATE",
            string actionName = "FM001",
            string details = "FreightMaster created",
            string module = "FreightMaster") =>
            new(actionDetail, actionCode, actionName, details, module);

        [Fact]
        public async Task Handle_ValidNotification_CallsGetCollectionWithAuditLogs()
        {
            SetupDefaultMocks();
            var sut = CreateSut();

            await sut.Handle(CreateTestEvent(), CancellationToken.None);

            _mockMongoDbContext.Verify(
                c => c.GetCollection<dynamic>("AuditLogs"),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_CallsGetUserId()
        {
            SetupDefaultMocks();
            var sut = CreateSut();

            await sut.Handle(CreateTestEvent(), CancellationToken.None);

            _mockIpAddressService.Verify(s => s.GetUserId(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_CallsGetSystemTimeZone()
        {
            SetupDefaultMocks();
            var sut = CreateSut();

            await sut.Handle(CreateTestEvent(), CancellationToken.None);

            _mockTimeZoneService.Verify(s => s.GetSystemTimeZone(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_CallsGetCurrentTime()
        {
            SetupDefaultMocks();
            var sut = CreateSut();

            await sut.Handle(CreateTestEvent(), CancellationToken.None);

            _mockTimeZoneService.Verify(
                s => s.GetCurrentTime("India Standard Time"),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_CallsGetUserOS()
        {
            SetupDefaultMocks();
            var sut = CreateSut();

            await sut.Handle(CreateTestEvent(), CancellationToken.None);

            _mockIpAddressService.Verify(s => s.GetUserOS(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_CallsGetSystemIPAddress()
        {
            SetupDefaultMocks();
            var sut = CreateSut();

            await sut.Handle(CreateTestEvent(), CancellationToken.None);

            _mockIpAddressService.Verify(s => s.GetSystemIPAddress(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_CallsGetUserAgent()
        {
            SetupDefaultMocks();
            var sut = CreateSut();

            await sut.Handle(CreateTestEvent(), CancellationToken.None);

            _mockIpAddressService.Verify(s => s.GetUserAgent(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidNotification_CallsGetUserName()
        {
            SetupDefaultMocks();
            var sut = CreateSut();

            await sut.Handle(CreateTestEvent(), CancellationToken.None);

            _mockIpAddressService.Verify(s => s.GetUserName(), Times.Once);
        }

        [Fact]
        public void Handle_ShouldImplementINotificationHandler()
        {
            var sut = CreateSut();
            sut.Should().BeAssignableTo<MediatR.INotificationHandler<AuditLogsDomainEvent>>();
        }
    }
}
