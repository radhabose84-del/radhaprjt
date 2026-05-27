using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Dto;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetAllReturnReasons;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonAutoComplete;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonById;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Queries.GetReturnReasonsByReturnType;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Queries;

public sealed class ReturnReasonQueryHandlerTests
{
    private readonly Mock<IReturnReasonQueryRepository> _mockRepo = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    [Fact]
    public async Task GetAll_ReturnsPagedResult()
    {
        var items = new List<ReturnReasonDto> { ReturnReasonBuilders.ValidDto(1) };
        _mockRepo.Setup(r => r.GetAllAsync(1, 20, null, It.IsAny<CancellationToken>())).ReturnsAsync((items, 1));

        var handler = new GetAllReturnReasonsQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetAllReturnReasonsQuery(1, 20, null), CancellationToken.None);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_Found_ReturnsDto()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ReturnReasonBuilders.ValidDto(1));

        var handler = new GetReturnReasonByIdQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetReturnReasonByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AutoComplete_ReturnsItems()
    {
        var items = new List<ReturnReasonLookupDto> { ReturnReasonBuilders.ValidLookupDto(1) };
        _mockRepo.Setup(r => r.AutocompleteAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(items);

        var handler = new GetReturnReasonAutoCompleteQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetReturnReasonAutoCompleteQuery("Moisture"), CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByReturnType_ReturnsCascadingList()
    {
        var items = new List<ReturnReasonLookupDto> { ReturnReasonBuilders.ValidLookupDto(1) };
        _mockRepo.Setup(r => r.GetByReturnTypeIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(items);

        var handler = new GetReturnReasonsByReturnTypeQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetReturnReasonsByReturnTypeQuery(1), CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
