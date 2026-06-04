using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetAllPurchaseReturns;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnAutoComplete;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnById;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetReturnableQtyByGrn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetReturnablePosByVendor;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetReturnableGrnsByVendorPo;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnPending;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Queries;

public sealed class PurchaseReturnQueryHandlerTests
{
    private readonly Mock<IPurchaseReturnQueryRepository> _mockRepo = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    [Fact]
    public async Task GetAll_ReturnsPagedResult()
    {
        var items = new List<PurchaseReturnListItemDto> { PurchaseReturnBuilders.ValidListItemDto(1) };
        _mockRepo.Setup(r => r.GetAllAsync(1, 20, null, It.IsAny<CancellationToken>())).ReturnsAsync((items, 1));

        var handler = new GetAllPurchaseReturnsQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetAllPurchaseReturnsQuery(1, 20, null), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
    }

    [Fact]
    public async Task GetById_Found_ReturnsDto()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(PurchaseReturnBuilders.ValidHeaderDto(1));

        var handler = new GetPurchaseReturnByIdQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetPurchaseReturnByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.RtvNumber.Should().Be("RTV/2026/0001");
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((PurchaseReturnHeaderDto?)null);

        var handler = new GetPurchaseReturnByIdQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetPurchaseReturnByIdQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetReturnableQty_ReturnsRows()
    {
        var rows = new List<ReturnableQtyDto> { PurchaseReturnBuilders.ValidReturnableQtyDto(1) };
        _mockRepo.Setup(r => r.GetReturnableQtyByGrnAsync(200, It.IsAny<CancellationToken>())).ReturnsAsync(rows);

        var handler = new GetReturnableQtyByGrnQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetReturnableQtyByGrnQuery(200), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].ReturnableQty.Should().Be(75m);
    }

    [Fact]
    public async Task AutoComplete_ReturnsItems()
    {
        var items = new List<PurchaseReturnListItemDto> { PurchaseReturnBuilders.ValidListItemDto(1) };
        _mockRepo.Setup(r => r.AutocompleteAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(items);

        var handler = new GetPurchaseReturnAutoCompleteQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetPurchaseReturnAutoCompleteQuery("RTV"), CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPosByVendor_ReturnsPos()
    {
        var pos = new List<PurchaseReturnPoLookupDto>
        {
            new() { PoId = 59, PoNumber = "PO-KNIT-Loc-04" },
            new() { PoId = 52, PoNumber = "PO-KNIT-Loc-01" }
        };
        _mockRepo.Setup(r => r.GetPosByVendorAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(pos);

        var handler = new GetReturnablePosByVendorQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetReturnablePosByVendorQuery(9), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].PoNumber.Should().Be("PO-KNIT-Loc-04");
    }

    [Fact]
    public async Task GetPosByVendor_PassesVendorIdToRepository()
    {
        _mockRepo.Setup(r => r.GetPosByVendorAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<PurchaseReturnPoLookupDto>());

        var handler = new GetReturnablePosByVendorQueryHandler(_mockRepo.Object, _mockMediator.Object);
        await handler.Handle(new GetReturnablePosByVendorQuery(9), CancellationToken.None);

        _mockRepo.Verify(r => r.GetPosByVendorAsync(9, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetGrnsByVendorPo_ReturnsGrns()
    {
        var grns = new List<PurchaseReturnGrnLookupDto>
        {
            new() { GrnHeaderId = 1024, GrnNo = "GRN-37-16" }
        };
        _mockRepo.Setup(r => r.GetGrnsByVendorPoAsync(9, 59, It.IsAny<CancellationToken>())).ReturnsAsync(grns);

        var handler = new GetReturnableGrnsByVendorPoQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetReturnableGrnsByVendorPoQuery(9, 59), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].GrnNo.Should().Be("GRN-37-16");
    }

    [Fact]
    public async Task GetGrnsByVendorPo_PassesVendorAndPoToRepository()
    {
        _mockRepo.Setup(r => r.GetGrnsByVendorPoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<PurchaseReturnGrnLookupDto>());

        var handler = new GetReturnableGrnsByVendorPoQueryHandler(_mockRepo.Object, _mockMediator.Object);
        await handler.Handle(new GetReturnableGrnsByVendorPoQuery(9, 59), CancellationToken.None);

        _mockRepo.Verify(r => r.GetGrnsByVendorPoAsync(9, 59, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPending_FiltersToCurrentApprover_AndEnriches()
    {
        var rows = new List<PurchaseReturnPendingDto>
        {
            new() { Id = 1, RtvNumber = "RTV/2026/0001", VendorId = 1052 },
            new() { Id = 2, RtvNumber = "RTV/2026/0002", VendorId = 1052 }
        };
        _mockRepo.Setup(r => r.GetPendingAsync(1, 20, null, It.IsAny<CancellationToken>())).ReturnsAsync(rows);

        var mockWf = new Mock<IWorkflowLookup>(MockBehavior.Loose);
        mockWf.Setup(w => w.GetApproverListAsync("Purchase Return", It.IsAny<IEnumerable<int>>()))
              .ReturnsAsync(new List<ApproverListDto>
              {
                  new() { ModuleTransactionId = 1, ApproverValue = "363", ApprovalRequestId = 4629, IsEdit = 0 },
                  new() { ModuleTransactionId = 2, ApproverValue = "999", ApprovalRequestId = 4630, IsEdit = 0 }
              });

        var mockUsers = new Mock<IUserLookup>(MockBehavior.Loose);
        mockUsers.Setup(u => u.GetAllUserAsync())
                 .ReturnsAsync(new List<UserLookupDto> { new() { UserId = 363, UserName = "Superadmin" } });

        var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
        mockIp.Setup(i => i.GetUserId()).Returns(363);

        var handler = new GetPurchaseReturnPendingQueryHandler(
            _mockRepo.Object, mockWf.Object, mockUsers.Object, mockIp.Object, _mockMediator.Object);

        var (items, total) = await handler.Handle(new GetPurchaseReturnPendingQuery(1, 20, null), CancellationToken.None);

        items.Should().HaveCount(1);                       // only RTV 1 (approver = current user 363)
        items[0].Id.Should().Be(1);
        items[0].ApproverId.Should().Be(363);
        items[0].ApproverName.Should().Be("Superadmin");
        items[0].ApprovalRequestHeaderId.Should().Be(4629);
        total.Should().Be(1);
    }

    [Fact]
    public async Task GetPending_NoRows_ReturnsEmpty()
    {
        _mockRepo.Setup(r => r.GetPendingAsync(1, 20, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<PurchaseReturnPendingDto>());

        var handler = new GetPurchaseReturnPendingQueryHandler(
            _mockRepo.Object,
            new Mock<IWorkflowLookup>(MockBehavior.Loose).Object,
            new Mock<IUserLookup>(MockBehavior.Loose).Object,
            new Mock<IIPAddressService>(MockBehavior.Loose).Object,
            _mockMediator.Object);

        var (items, total) = await handler.Handle(new GetPurchaseReturnPendingQuery(1, 20, null), CancellationToken.None);

        items.Should().BeEmpty();
        total.Should().Be(0);
    }
}
