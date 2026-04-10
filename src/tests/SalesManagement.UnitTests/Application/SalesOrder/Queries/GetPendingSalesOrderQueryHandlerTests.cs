using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrder;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrder.Queries;

public sealed class GetPendingSalesOrderQueryHandlerTests
{
    private readonly Mock<ISalesOrderQueryRepository> _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
    private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
    private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

    private GetPendingSalesOrderQueryHandler CreateSut() =>
        new(_mockRepo.Object, _mockPartyLookup.Object, _mockUnitLookup.Object,
            _mockMediator.Object, _mockWorkflowLookup.Object, _mockUserLookup.Object,
            _mockIpService.Object);

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _mockRepo
            .Setup(r => r.GetPendingSalesOrderAsync(1, 15, null))
            .ReturnsAsync((new List<PendingSalesOrderDto>(), 0));

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var query = new GetPendingSalesOrderQuery { PageNumber = 1, PageSize = 15 };
        var (items, total) = await CreateSut().Handle(query, CancellationToken.None);

        items.Should().BeEmpty();
        total.Should().Be(0);
    }

    [Fact]
    public async Task Handle_EmptyResult_PublishesAuditEvent()
    {
        _mockRepo
            .Setup(r => r.GetPendingSalesOrderAsync(1, 15, null))
            .ReturnsAsync((new List<PendingSalesOrderDto>(), 0));

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateSut().Handle(new GetPendingSalesOrderQuery(), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
