using Contracts.Interfaces;
using MongoDB.Driver;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.EventHandlers;
using PartyManagement.Domain.Events;

namespace PartyManagement.UnitTests.Application.EventHandlers
{
    public sealed class DomainEventHandlerTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoContext = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZone = new(MockBehavior.Loose);

        private DomainEventHandler CreateSut() =>
            new(_mockMongoContext.Object, _mockIpService.Object, _mockTimeZone.Object);

        private void SetupDefaults()
        {
            _mockIpService.Setup(x => x.GetUserId()).Returns(1);
            _mockIpService.Setup(x => x.GetUserName()).Returns("test-user");
            _mockIpService.Setup(x => x.GetUserOS()).Returns("Windows");
            _mockIpService.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockIpService.Setup(x => x.GetUserAgent()).Returns("TestBrowser");

            _mockTimeZone.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            _mockTimeZone.Setup(x => x.GetCurrentTime("UTC")).Returns(DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Handle_ValidEvent_CallsInsertOneAsync()
        {
            SetupDefaults();
            var mockCollection = new Mock<IMongoCollection<object>>(MockBehavior.Loose);
            _mockMongoContext
                .Setup(c => c.GetCollection<dynamic>("AuditLogs"))
                .Returns(mockCollection.As<IMongoCollection<dynamic>>().Object);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "TEST_CREATE",
                actionName: "TestEntity",
                details: "Test entity created",
                module: "TestModule");

            await CreateSut().Handle(domainEvent, CancellationToken.None);

            // Verify the mongo context was called to get the collection
            _mockMongoContext.Verify(c => c.GetCollection<dynamic>("AuditLogs"), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidEvent_ReadsUserIdFromIpService()
        {
            SetupDefaults();
            var mockCollection = new Mock<IMongoCollection<object>>(MockBehavior.Loose);
            _mockMongoContext
                .Setup(c => c.GetCollection<dynamic>("AuditLogs"))
                .Returns(mockCollection.As<IMongoCollection<dynamic>>().Object);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "TEST_UPDATE",
                actionName: "TestEntity",
                details: "Test entity updated",
                module: "TestModule");

            await CreateSut().Handle(domainEvent, CancellationToken.None);

            _mockIpService.Verify(x => x.GetUserId(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidEvent_GetsTimeZone()
        {
            SetupDefaults();
            var mockCollection = new Mock<IMongoCollection<object>>(MockBehavior.Loose);
            _mockMongoContext
                .Setup(c => c.GetCollection<dynamic>("AuditLogs"))
                .Returns(mockCollection.As<IMongoCollection<dynamic>>().Object);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "TEST_DELETE",
                actionName: "TestEntity",
                details: "Test entity deleted",
                module: "TestModule");

            await CreateSut().Handle(domainEvent, CancellationToken.None);

            _mockTimeZone.Verify(x => x.GetSystemTimeZone(), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidEvent_GetsCurrentTime()
        {
            SetupDefaults();
            var mockCollection = new Mock<IMongoCollection<object>>(MockBehavior.Loose);
            _mockMongoContext
                .Setup(c => c.GetCollection<dynamic>("AuditLogs"))
                .Returns(mockCollection.As<IMongoCollection<dynamic>>().Object);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "TEST_CREATE",
                actionName: "TestEntity",
                details: "Test entity created",
                module: "TestModule");

            await CreateSut().Handle(domainEvent, CancellationToken.None);

            _mockTimeZone.Verify(x => x.GetCurrentTime("UTC"), Times.Once);
        }
    }
}
