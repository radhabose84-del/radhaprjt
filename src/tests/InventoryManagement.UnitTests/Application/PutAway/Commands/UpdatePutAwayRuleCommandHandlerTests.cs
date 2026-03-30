using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using InventoryManagement.Application.Item.PutAway.Commands.UpdatePutAwayRule;
using InventoryManagement.Domain.Entities.Item.PutAway;
using MediatR;

namespace InventoryManagement.UnitTests.Application.PutAway.Commands
{
    public sealed class UpdatePutAwayRuleCommandHandlerTests
    {
        private readonly Mock<IPutAwayRuleCommandRepository> _mockRepo = new(MockBehavior.Strict);

        private UpdatePutAwayRuleCommandHandler CreateSut() =>
            new(_mockRepo.Object);

        private static UpdatePutAwayRuleCommand BuildCommand(int id = 1) =>
            new(id, new CreatePutAwayRuleRequest
            {
                UnitId = 1,
                WarehouseId = 2,
                ItemGroupId = 3,
                ItemCategoryId = 4,
                IsActive = 1,
                Strategies = new List<CreatePutAwayStrategyRequest>()
            });

        [Fact]
        public async Task Handle_ExistingRule_ReturnsUnit()
        {
            var cmd = BuildCommand(1);
            var entity = new PutAwayRule { Id = 1, Strategies = new List<PutAwayStrategy>() };

            _mockRepo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockRepo.Setup(r => r.ExistsScopeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockRepo.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(cmd, CancellationToken.None);

            result.Should().Be(Unit.Value);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsEntityNotFoundException()
        {
            var cmd = BuildCommand(99);

            _mockRepo.Setup(r => r.GetByIdAsync(99, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PutAwayRule?)null);

            Func<Task> act = async () => await CreateSut().Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task Handle_DuplicateScope_ThrowsEntityAlreadyExistsException()
        {
            var cmd = BuildCommand(1);
            var entity = new PutAwayRule { Id = 1, Strategies = new List<PutAwayStrategy>() };

            _mockRepo.Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockRepo.Setup(r => r.ExistsScopeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<EntityAlreadyExistsException>();
        }
    }
}
