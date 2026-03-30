using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleById;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.PutAway.Queries
{
    public sealed class GetPutAwayRuleByIdQueryHandlerTests
    {
        private readonly Mock<IPutAwayRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetPutAwayRuleByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var raw = new PutAwayRuleDetailDto { Id = 1 };
            var dto = new PutAwayRuleDetailDto { Id = 1 };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(raw);
            _mockMapper.Setup(m => m.Map<PutAwayRuleDetailDto>(raw)).Returns(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetPutAwayRuleByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsEntityNotFoundException()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PutAwayRuleDetailDto?)null);

            Func<Task> act = async () =>
                await CreateSut().Handle(new GetPutAwayRuleByIdQuery { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var raw = new PutAwayRuleDetailDto { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(raw);
            _mockMapper.Setup(m => m.Map<PutAwayRuleDetailDto>(raw)).Returns(raw);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetPutAwayRuleByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
