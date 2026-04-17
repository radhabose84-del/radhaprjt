using Contracts.Interfaces;
using MongoDB.Driver;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.EventHandlers;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.AuditLog.EventHandlers;

public sealed class DomainEventHandlerTests
{
    private readonly Mock<IMongoDbContext> _mockMongoDbContext = new(MockBehavior.Strict);
    private readonly Mock<IIPAddressService> _mockIpAddressService = new(MockBehavior.Strict);
    private readonly Mock<ITimeZoneService> _mockTimeZoneService = new(MockBehavior.Strict);

    private DomainEventHandler CreateSut() =>
        new(_mockMongoDbContext.Object, _mockIpAddressService.Object, _mockTimeZoneService.Object);

    private void SetupHappyPath()
    {
        _mockIpAddressService.Setup(s => s.GetUserId()).Returns(1);
        _mockIpAddressService.Setup(s => s.GetUserOS()).Returns("Windows 11");
        _mockIpAddressService.Setup(s => s.GetSystemIPAddress()).Returns("192.168.1.100");
        _mockIpAddressService.Setup(s => s.GetUserAgent()).Returns("Chrome");
        _mockIpAddressService.Setup(s => s.GetUserName()).Returns("test-user");

        _mockTimeZoneService.Setup(s => s.GetSystemTimeZone()).Returns("India Standard Time");
        _mockTimeZoneService.Setup(s => s.GetCurrentTime("India Standard Time"))
            .Returns(DateTimeOffset.UtcNow);

        var mockCollection = new Mock<IMongoCollection<dynamic>>(MockBehavior.Loose);
        _mockMongoDbContext
            .Setup(c => c.GetCollection<dynamic>("AuditLogs"))
            .Returns(mockCollection.Object);
    }

    [Fact]
    public async Task Handle_ValidEvent_CallsGetCollectionWithAuditLogs()
    {
        SetupHappyPath();
        var notification = new AuditLogsDomainEvent("Create", "ENTITY_CREATE", "CODE001", "Created successfully", "TestModule");

        await CreateSut().Handle(notification, CancellationToken.None);

        _mockMongoDbContext.Verify(c => c.GetCollection<dynamic>("AuditLogs"), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidEvent_ReadsUserIdFromIPAddressService()
    {
        SetupHappyPath();
        var notification = new AuditLogsDomainEvent("Update", "ENTITY_UPDATE", "1", "Updated", "TestModule");

        await CreateSut().Handle(notification, CancellationToken.None);

        _mockIpAddressService.Verify(s => s.GetUserId(), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidEvent_ReadsTimeZone()
    {
        SetupHappyPath();
        var notification = new AuditLogsDomainEvent("Create", "ENTITY_CREATE", "CODE001", "Created", "TestModule");

        await CreateSut().Handle(notification, CancellationToken.None);

        _mockTimeZoneService.Verify(s => s.GetSystemTimeZone(), Times.Once);
        _mockTimeZoneService.Verify(s => s.GetCurrentTime("India Standard Time"), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidEvent_ReadsUserAgentAndOS()
    {
        SetupHappyPath();
        var notification = new AuditLogsDomainEvent("Delete", "ENTITY_DELETE", "1", "Deleted", "TestModule");

        await CreateSut().Handle(notification, CancellationToken.None);

        _mockIpAddressService.Verify(s => s.GetUserOS(), Times.Once);
        _mockIpAddressService.Verify(s => s.GetUserAgent(), Times.Once);
        _mockIpAddressService.Verify(s => s.GetSystemIPAddress(), Times.Once);
        _mockIpAddressService.Verify(s => s.GetUserName(), Times.Once);
    }
}
