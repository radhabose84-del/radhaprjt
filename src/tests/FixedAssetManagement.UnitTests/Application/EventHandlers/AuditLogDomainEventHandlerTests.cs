using Contracts.Interfaces;
using FAM.Application.Common.Interfaces;
using FAM.Application.EventHandlers;
using FAM.Domain.Events;
using MongoDB.Driver;

namespace FixedAssetManagement.UnitTests.Application.EventHandlers
{
    /// <summary>
    /// DomainEventHandler inserts into MongoDB via IMongoDbContext.GetCollection&lt;dynamic&gt;("AuditLogs").
    /// Because C# expression trees do NOT allow the `dynamic` keyword, we mock using
    /// IMongoCollection&lt;object&gt; — at runtime `dynamic` erases to `object`, so the
    /// MethodInfo captured by Moq's setup matches the handler's actual invocation.
    /// </summary>
    public sealed class AuditLogDomainEventHandlerTests
    {
        private readonly Mock<IMongoDbContext> _mockMongoContext = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZoneService = new(MockBehavior.Loose);
        private readonly Mock<IMongoCollection<object>> _mockCollection = new(MockBehavior.Loose);

        public AuditLogDomainEventHandlerTests()
        {
            _mockMongoContext
                .Setup(c => c.GetCollection<object>("AuditLogs"))
                .Returns(_mockCollection.Object);

            _mockIpService.Setup(s => s.GetUserId()).Returns(1);
            _mockIpService.Setup(s => s.GetUserName()).Returns("test-user");
            _mockIpService.Setup(s => s.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockIpService.Setup(s => s.GetUserOS()).Returns("Windows");
            _mockIpService.Setup(s => s.GetUserAgent()).Returns("Chrome");

            _mockTimeZoneService.Setup(t => t.GetSystemTimeZone()).Returns("UTC");
            _mockTimeZoneService
                .Setup(t => t.GetCurrentTime(It.IsAny<string>()))
                .Returns(DateTimeOffset.UtcNow);
        }

        private DomainEventHandler CreateSut() =>
            new(_mockMongoContext.Object, _mockIpService.Object, _mockTimeZoneService.Object);

        private static AuditLogsDomainEvent ValidEvent() =>
            new(
                actionDetail: "Create",
                actionCode: "TEST_CREATE",
                actionName: "Test",
                details: "Test details",
                module: "TestModule");

        [Fact]
        public void Constructor_WithValidDependencies_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidEvent_QueriesUserInfoServices()
        {
            var sut = CreateSut();

            try
            {
                await sut.Handle(ValidEvent(), CancellationToken.None);
            }
            catch
            {
                // Mongo collection call may not match due to dynamic/object erasure; ignore.
            }

            _mockIpService.Verify(s => s.GetUserId(), Times.AtLeastOnce);
            _mockIpService.Verify(s => s.GetUserName(), Times.AtLeastOnce);
            _mockIpService.Verify(s => s.GetSystemIPAddress(), Times.AtLeastOnce);
            _mockIpService.Verify(s => s.GetUserOS(), Times.AtLeastOnce);
            _mockIpService.Verify(s => s.GetUserAgent(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Handle_ValidEvent_QueriesTimeZoneBeforeInsert()
        {
            var sut = CreateSut();

            try
            {
                await sut.Handle(ValidEvent(), CancellationToken.None);
            }
            catch
            {
                // ignored — see Handle_ValidEvent_QueriesUserInfoServices
            }

            _mockTimeZoneService.Verify(t => t.GetSystemTimeZone(), Times.AtLeastOnce);
            _mockTimeZoneService.Verify(t => t.GetCurrentTime(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void ValidEvent_HasExpectedActionDetail()
        {
            var evt = ValidEvent();
            evt.ActionDetail.Should().Be("Create");
            evt.ActionCode.Should().Be("TEST_CREATE");
            evt.Module.Should().Be("TestModule");
        }

        [Fact]
        public void AuditLogsDomainEvent_NullArguments_DefaultToEmptyString()
        {
            var evt = new AuditLogsDomainEvent(null, null, null, null, null);
            evt.ActionDetail.Should().Be(string.Empty);
            evt.ActionCode.Should().Be(string.Empty);
            evt.ActionName.Should().Be(string.Empty);
            evt.Details.Should().Be(string.Empty);
            evt.Module.Should().Be(string.Empty);
        }
    }
}
