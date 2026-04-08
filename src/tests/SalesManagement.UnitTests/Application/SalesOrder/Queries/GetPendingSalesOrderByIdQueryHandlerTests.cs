using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderById;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.SalesOrder.Queries;

public sealed class GetPendingSalesOrderByIdQueryHandlerTests
{
    private readonly Mock<ISalesOrderQueryRepository> _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
    private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
    private readonly Mock<IPaymentTermLookup> _mockPaymentTermLookup = new(MockBehavior.Loose);
    private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
    private readonly Mock<IHSNLookup> _mockHsnLookup = new(MockBehavior.Loose);
    private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
    private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
    private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

    private GetPendingSalesOrderByIdQueryHandler CreateSut() =>
        new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
            _mockPartyLookup.Object, _mockUnitLookup.Object, _mockPaymentTermLookup.Object,
            _mockItemLookup.Object, _mockHsnLookup.Object, _mockUomLookup.Object,
            _mockWorkflowLookup.Object, _mockUserLookup.Object, _mockIpService.Object);

    [Fact]
    public async Task Handle_NotFound_ReturnsNull()
    {
        _mockRepo
            .Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((SalesOrderHeaderDto?)null);

        var query = new GetPendingSalesOrderByIdQuery { Id = 99 };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
