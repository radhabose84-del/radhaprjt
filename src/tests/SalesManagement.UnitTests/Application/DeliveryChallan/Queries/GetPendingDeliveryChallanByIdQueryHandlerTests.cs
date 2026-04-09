using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Application.DeliveryChallan.Queries.GetPendingDeliveryChallanById;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.DeliveryChallan.Queries;

public sealed class GetPendingDeliveryChallanByIdQueryHandlerTests
{
    private readonly Mock<IDeliveryChallanQueryRepository> _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
    private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

    private GetPendingDeliveryChallanByIdQueryHandler CreateSut() =>
        new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
            _mockWorkflowLookup.Object, _mockUserLookup.Object, _mockIpService.Object);

    [Fact]
    public async Task Handle_NotFound_ReturnsNull()
    {
        _mockRepo
            .Setup(r => r.GetPendingByIdAsync(99))
            .ReturnsAsync((DeliveryChallanHeaderDto?)null);

        var query = new GetPendingDeliveryChallanByIdQuery { Id = 99 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
