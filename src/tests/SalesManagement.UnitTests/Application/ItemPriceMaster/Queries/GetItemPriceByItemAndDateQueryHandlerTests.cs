using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceByItemAndDate;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ItemPriceMaster.Queries;

public sealed class GetItemPriceByItemAndDateQueryHandlerTests
{
    private readonly Mock<IItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
    private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
    private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

    private GetItemPriceByItemAndDateQueryHandler CreateSut() =>
        new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

    [Fact]
    public async Task Handle_ReturnsSuccess()
    {
        var data = new List<ItemPriceMasterDto> { new() { Id = 1 } };

        _mockQueryRepo
            .Setup(r => r.GetByItemAndDateAsync(1, It.IsAny<DateOnly>()))
            .ReturnsAsync(data);

        _mockMapper
            .Setup(m => m.Map<List<ItemPriceMasterDto>>(It.IsAny<List<ItemPriceMasterDto>>()))
            .Returns(data);

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var query = new GetItemPriceByItemAndDateQuery { ItemId = 1, Date = new DateOnly(2026, 1, 1) };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsSuccess()
    {
        _mockQueryRepo
            .Setup(r => r.GetByItemAndDateAsync(1, It.IsAny<DateOnly>()))
            .ReturnsAsync(new List<ItemPriceMasterDto>());

        _mockMapper
            .Setup(m => m.Map<List<ItemPriceMasterDto>>(It.IsAny<List<ItemPriceMasterDto>>()))
            .Returns(new List<ItemPriceMasterDto>());

        _mockMediator
            .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var query = new GetItemPriceByItemAndDateQuery { ItemId = 1, Date = new DateOnly(2026, 1, 1) };
        var result = await CreateSut().Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
