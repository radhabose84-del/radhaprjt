using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleItemId;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.PutAway.Queries
{
    public sealed class GetPutAwayRuleItemIdQueryHandlerTests
    {
        private readonly Mock<IPutAwayRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPutAwayRuleItemIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingRules()
        {
            var rawList = new List<GetPutAwayRuleItemIdDto?> { new() { ItemId = 1 } };
            var mappedList = new List<GetPutAwayRuleItemIdDto> { new() { ItemId = 1 } };

            _mockQueryRepo.Setup(r => r.GetPutAwayRuleDetailsAsync(
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .ReturnsAsync(rawList);
            _mockMapper.Setup(m => m.Map<List<GetPutAwayRuleItemIdDto>>(rawList)).Returns(mappedList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetPutAwayRuleItemIdQuery { ItemIds = new List<int> { 1 }, WarehouseIds = new List<int> { 2 } },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].ItemId.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyItemIds_ReturnsEmpty()
        {
            _mockQueryRepo.Setup(r => r.GetPutAwayRuleDetailsAsync(
                    It.IsAny<List<int>>(), It.IsAny<List<int>>()))
                .ReturnsAsync(new List<GetPutAwayRuleItemIdDto?>());
            _mockMapper.Setup(m => m.Map<List<GetPutAwayRuleItemIdDto>>(It.IsAny<object>()))
                .Returns(new List<GetPutAwayRuleItemIdDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetPutAwayRuleItemIdQuery { ItemIds = null, WarehouseIds = null },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
