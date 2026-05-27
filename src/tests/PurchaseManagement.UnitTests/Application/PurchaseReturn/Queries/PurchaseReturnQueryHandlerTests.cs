using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetAllPurchaseReturns;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnAutoComplete;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnById;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetReturnableQtyByGrn;
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
}
