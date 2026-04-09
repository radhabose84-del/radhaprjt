using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderAmendment;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrder.Queries;

public sealed class GetPendingSalesOrderAmendmentQueryHandlerTests
{
    private readonly Mock<ISalesOrderAmendmentQueryRepository> _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
    private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetPendingSalesOrderAmendmentQueryHandler CreateSut() =>
        new(_mockRepo.Object, _mockWorkflowLookup.Object, _mockUserLookup.Object,
            _mockIpService.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _mockRepo
            .Setup(r => r.GetPendingAsync(1, 15, null))
            .ReturnsAsync((new List<PendingSalesOrderAmendmentDto>(), 0));

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var query = new GetPendingSalesOrderAmendmentQuery { PageNumber = 1, PageSize = 15 };
        var (items, total) = await CreateSut().Handle(query, CancellationToken.None);

        items.Should().BeEmpty();
        total.Should().Be(0);
    }
}
