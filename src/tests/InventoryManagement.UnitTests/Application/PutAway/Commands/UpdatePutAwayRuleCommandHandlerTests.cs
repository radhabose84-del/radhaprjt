using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using InventoryManagement.Application.Item.PutAway.Commands.UpdatePutAwayRule;
using InventoryManagement.Domain.Entities.Item.PutAway;
using MediatR;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Application.PutAway.Commands
{
    public sealed class UpdatePutAwayRuleCommandHandlerTests
    {
        private readonly Mock<IPutAwayRuleCommandRepository> _mockCmd = new(MockBehavior.Strict);
        private readonly Mock<IPutAwayRuleQueryRepository> _mockQuery = new(MockBehavior.Strict);

        private UpdatePutAwayRuleCommandHandler CreateSut() =>
            new(_mockCmd.Object, _mockQuery.Object);

        private static UpdatePutAwayRuleCommand BuildCommand(int id = 1, byte isActive = 1) =>
            new(id, new CreatePutAwayRuleRequest
            {
                UnitId = 1,
                WarehouseId = 2,
                ItemGroupId = 3,
                ItemCategoryId = 4,
                ItemId = null,
                IsActive = isActive,
                Strategies = new List<CreatePutAwayStrategyRequest>
                {
                    new() { PriorityId = 1, StorageTypeId = 10, TargetId = 20 }
                }
            });

        private static PutAwayRule BuildExistingEntity(int id = 1) =>
            new()
            {
                Id = id,
                UnitId = 1,
                WarehouseId = 2,
                ItemGroupId = 3,
                ItemCategoryId = 4,
                ItemId = null,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Strategies = new List<PutAwayStrategy>
                {
                    new()
                    {
                        Id = 100,
                        PutAwayRuleId = id,
                        PriorityId = 1,
                        StorageTypeId = 10,
                        TargetId = 20,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }
                }
            };

        private void SetupHappyPath(int id = 1)
        {
            _mockQuery
                .Setup(r => r.IsPutAwayRuleLinkedAsync(id))
                .ReturnsAsync(false);

            _mockCmd
                .Setup(r => r.GetByIdAsync(id, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildExistingEntity(id));

            _mockCmd
                .Setup(r => r.ExistsScopeAsync(1, 2, 3, 4, null, id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCmd
                .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUnit()
        {
            SetupHappyPath();
            var cmd = BuildCommand();

            var result = await CreateSut().Handle(cmd, CancellationToken.None);

            result.Should().Be(Unit.Value);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsSave()
        {
            SetupHappyPath();
            var cmd = BuildCommand();

            await CreateSut().Handle(cmd, CancellationToken.None);

            _mockCmd.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsEntityNotFoundException()
        {
            _mockQuery
                .Setup(r => r.IsPutAwayRuleLinkedAsync(99))
                .ReturnsAsync(false);

            _mockCmd
                .Setup(r => r.GetByIdAsync(99, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PutAwayRule?)null);

            var cmd = BuildCommand(id: 99);

            Func<Task> act = async () => await CreateSut().Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task Handle_DuplicateScope_ThrowsEntityAlreadyExistsException()
        {
            _mockQuery
                .Setup(r => r.IsPutAwayRuleLinkedAsync(1))
                .ReturnsAsync(false);

            _mockCmd
                .Setup(r => r.GetByIdAsync(1, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildExistingEntity());

            _mockCmd
                .Setup(r => r.ExistsScopeAsync(1, 2, 3, 4, null, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var cmd = BuildCommand();

            Func<Task> act = async () => await CreateSut().Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<EntityAlreadyExistsException>();
        }

        [Fact]
        public async Task Handle_InactivateLinkedRecord_ThrowsExceptionRules()
        {
            _mockQuery
                .Setup(r => r.IsPutAwayRuleLinkedAsync(1))
                .ReturnsAsync(true);

            var cmd = BuildCommand(isActive: 0);

            Func<Task> act = async () => await CreateSut().Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*linked*");
        }
    }
}
