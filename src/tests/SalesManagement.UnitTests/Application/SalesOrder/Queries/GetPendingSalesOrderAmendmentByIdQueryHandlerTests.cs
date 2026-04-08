using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderAmendmentById;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrder.Queries;

public sealed class GetPendingSalesOrderAmendmentByIdQueryHandlerTests
{
    private readonly Mock<ISalesOrderAmendmentQueryRepository> _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
    private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetPendingSalesOrderAmendmentByIdQueryHandler CreateSut() =>
        new(_mockRepo.Object, _mockWorkflowLookup.Object, _mockUserLookup.Object,
            _mockIpService.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_NotFound_ReturnsNull()
    {
        _mockRepo
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((SalesOrderAmendmentHeaderDto?)null);

        var query = new GetPendingSalesOrderAmendmentByIdQuery { Id = 99 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
