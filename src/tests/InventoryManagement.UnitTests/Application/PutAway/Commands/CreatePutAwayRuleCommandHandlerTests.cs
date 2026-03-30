using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using InventoryManagement.Domain.Entities.Item.PutAway;
using AutoMapper;

namespace InventoryManagement.UnitTests.Application.PutAway.Commands
{
    public sealed class CreatePutAwayRuleCommandHandlerTests
    {
        private readonly Mock<IPutAwayRuleCommandRepository> _mockCmd = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreatePutAwayRuleCommandHandler CreateSut() =>
            new(_mockCmd.Object, _mockMapper.Object);

        private static CreatePutAwayRuleCommand BuildCommand() =>
            new(new CreatePutAwayRuleRequest
            {
                UnitId = 1,
                WarehouseId = 2,
                ItemGroupId = 3,
                ItemCategoryId = 4,
                ItemId = null,
                Strategies = new List<CreatePutAwayStrategyRequest>
                {
                    new() { PriorityId = 1, StorageTypeId = 10, TargetId = 20 }
                }
            });

        [Fact]
        public async Task Handle_NewScope_ReturnsNewId()
        {
            var cmd = BuildCommand();
            var entity = new PutAwayRule { Id = 5 };

            _mockCmd.Setup(r => r.ExistsScopeAsync(1, 2, 3, 4, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<PutAwayRule>(It.IsAny<object>())).Returns(entity);
            _mockMapper.Setup(m => m.Map<PutAwayStrategy>(It.IsAny<object>())).Returns(new PutAwayStrategy());
            _mockCmd.Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockCmd.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(cmd, CancellationToken.None);

            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_DuplicateScope_ThrowsEntityAlreadyExistsException()
        {
            var cmd = BuildCommand();

            _mockCmd.Setup(r => r.ExistsScopeAsync(1, 2, 3, 4, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            Func<Task> act = async () => await CreateSut().Handle(cmd, CancellationToken.None);

            await act.Should().ThrowAsync<EntityAlreadyExistsException>();
        }

        [Fact]
        public async Task Handle_NewScope_CallsAddAndSave()
        {
            var cmd = BuildCommand();
            var entity = new PutAwayRule { Id = 1 };

            _mockCmd.Setup(r => r.ExistsScopeAsync(1, 2, 3, 4, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<PutAwayRule>(It.IsAny<object>())).Returns(entity);
            _mockMapper.Setup(m => m.Map<PutAwayStrategy>(It.IsAny<object>())).Returns(new PutAwayStrategy());
            _mockCmd.Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockCmd.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(cmd, CancellationToken.None);

            _mockCmd.Verify(r => r.AddAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
            _mockCmd.Verify(r => r.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
