using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Command.DeleteRackMaster;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.RackMaster.Commands
{
    public sealed class DeleteRackMasterCommandHandlerTests
    {
        private readonly Mock<IRackMasterCommandRepository> _mockCmdRepo = new(MockBehavior.Strict);

        private DeleteRackMasterCommandHandler CreateSut() => new(_mockCmdRepo.Object);

        [Fact]
        public async Task Handle_ExistingEntity_ReturnsTrue()
        {
            var entity = RackMasterBuilders.ValidEntity();
            _mockCmdRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockCmdRepo.Setup(r => r.DeleteAsync(1, entity)).ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteRackMasterCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NonExistentEntity_ReturnsFalse()
        {
            _mockCmdRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((WarehouseManagement.Domain.Entities.RackMaster?)null);

            var result = await CreateSut().Handle(new DeleteRackMasterCommand { Id = 999 }, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_AlreadyDeletedEntity_ReturnsFalse()
        {
            var entity = RackMasterBuilders.ValidEntity();
            entity.IsDeleted = BaseEntity.IsDelete.Deleted;
            _mockCmdRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);

            var result = await CreateSut().Handle(new DeleteRackMasterCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
