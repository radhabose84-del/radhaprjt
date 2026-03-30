using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.DeleteWarehouseMaster;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.WarehouseMaster.Commands
{
    public sealed class DeleteWarehouseMasterCommandHandlerTests
    {
        private readonly Mock<IWarehouseMasterCommandRepository> _mockCmdRepo = new(MockBehavior.Strict);

        private DeleteWarehouseMasterCommandHandler CreateSut() => new(_mockCmdRepo.Object);

        [Fact]
        public async Task Handle_ExistingEntity_ReturnsTrue()
        {
            var entity = WarehouseMasterBuilders.ValidEntity();

            _mockCmdRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockCmdRepo.Setup(r => r.DeleteAsync(1, entity)).ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteWarehouseMasterCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NonExistentEntity_ReturnsFalse()
        {
            _mockCmdRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((WarehouseManagement.Domain.Entities.WarehouseMaster?)null);

            var result = await CreateSut().Handle(new DeleteWarehouseMasterCommand { Id = 999 }, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_AlreadyDeletedEntity_ReturnsFalse()
        {
            var entity = WarehouseMasterBuilders.ValidEntity();
            entity.IsDeleted = BaseEntity.IsDelete.Deleted;

            _mockCmdRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);

            var result = await CreateSut().Handle(new DeleteWarehouseMasterCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
