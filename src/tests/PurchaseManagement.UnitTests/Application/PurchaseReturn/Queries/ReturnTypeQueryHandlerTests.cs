using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Dto;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetAllReturnTypes;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetReturnTypeAutoComplete;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Queries.GetReturnTypeById;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseReturn.Queries;

public sealed class ReturnTypeQueryHandlerTests
{
    private readonly Mock<IReturnTypeQueryRepository> _mockRepo = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    [Fact]
    public async Task GetAll_ReturnsPagedResult()
    {
        var items = new List<ReturnTypeDto> { ReturnTypeBuilders.ValidDto(1) };
        _mockRepo.Setup(r => r.GetAllAsync(1, 20, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((items, 1));

        var handler = new GetAllReturnTypesQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetAllReturnTypesQuery(1, 20, null), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
    }

    [Fact]
    public async Task GetById_Found_ReturnsDto()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ReturnTypeBuilders.ValidDto(1));

        var handler = new GetReturnTypeByIdQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetReturnTypeByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Code.Should().Be("Rejected");
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((ReturnTypeDto?)null);

        var handler = new GetReturnTypeByIdQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetReturnTypeByIdQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AutoComplete_ReturnsItems()
    {
        var items = new List<ReturnTypeLookupDto> { ReturnTypeBuilders.ValidLookupDto(1) };
        _mockRepo.Setup(r => r.AutocompleteAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>())).ReturnsAsync(items);

        var handler = new GetReturnTypeAutoCompleteQueryHandler(_mockRepo.Object, _mockMediator.Object);
        var result = await handler.Handle(new GetReturnTypeAutoCompleteQuery("Rej"), CancellationToken.None);

        result.Should().HaveCount(1);
    }
}
