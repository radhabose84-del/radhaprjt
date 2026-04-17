using Contracts.Interfaces;
using MongoDB.Driver;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.EventHandlers;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.EventHandlers
{
    public sealed class DomainEventHandlerTests
    {
        private readonly Mock<IMongoDbContext> _mockMongo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);

        private DomainEventHandler CreateSut() =>
            new(_mockMongo.Object, _mockIp.Object, _mockTz.Object);

        [Fact]
        public async Task Handle_ValidNotification_CallsInsertOneAsync()
        {
            var notification = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "TEST_CREATE",
                actionName: "Test",
                details: "Test entity created.",
                module: "Test"
            );
            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test-user");
            _mockIp.Setup(i => i.GetUserOS()).Returns("Windows");
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockIp.Setup(i => i.GetUserAgent()).Returns("UnitTest");
            _mockTz.Setup(t => t.GetSystemTimeZone()).Returns("Asia/Kolkata");
            _mockTz.Setup(t => t.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

            var mockCollection = new Mock<IMongoCollection<dynamic>>();
            _mockMongo.Setup(m => m.GetCollection<dynamic>("AuditLogs")).Returns(mockCollection.Object);

            await CreateSut().Handle(notification, CancellationToken.None);

            mockCollection.Verify(
                c => c.InsertOneAsync(It.IsAny<object>(), null, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void CanInstantiate()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
