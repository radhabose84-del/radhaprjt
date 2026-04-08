using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Queries.GetInvoicePending;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.Invoice.Queries;

public sealed class GetInvoicePendingQueryHandlerTests
{
    private readonly Mock<IInvoiceQueryRepository> _mockRepo = new(MockBehavior.Strict);
    private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
    private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
    private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
    private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
    private readonly Mock<IFinancialYearLookup> _mockFinYearLookup = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
    private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
    private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
    private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

    private GetInvoicePendingQueryHandler CreateSut() =>
        new(_mockRepo.Object, _mockPartyLookup.Object, _mockUnitLookup.Object,
            _mockItemLookup.Object, _mockUomLookup.Object, _mockFinYearLookup.Object,
            _mockMediator.Object, _mockWorkflowLookup.Object, _mockUserLookup.Object,
            _mockIpService.Object);

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        _mockRepo
            .Setup(r => r.GetInvoicePendingAsync(1, 15, null))
            .ReturnsAsync((new List<GetInvoicePendingDto>(), 0));

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var query = new GetInvoicePendingQuery { PageNumber = 1, PageSize = 15 };
        var (items, total) = await CreateSut().Handle(query, CancellationToken.None);

        items.Should().BeEmpty();
        total.Should().Be(0);
    }

    [Fact]
    public async Task Handle_EmptyResult_PublishesAuditEvent()
    {
        _mockRepo
            .Setup(r => r.GetInvoicePendingAsync(1, 15, null))
            .ReturnsAsync((new List<GetInvoicePendingDto>(), 0));

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CreateSut().Handle(new GetInvoicePendingQuery(), CancellationToken.None);

        _mockMediator.Verify(
            m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
