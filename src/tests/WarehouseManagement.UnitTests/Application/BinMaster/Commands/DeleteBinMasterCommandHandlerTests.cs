using WarehouseManagement.Application.BinMaster.Command.DeleteBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Application.BinMaster.Commands
{
    public sealed class DeleteBinMasterCommandHandlerTests
    {
        private readonly Mock<IBinMasterCommandRepository> _mockCmdRepo = new(MockBehavior.Strict);

        private DeleteBinMasterCommandHandler CreateSut() => new(_mockCmdRepo.Object);

        [Fact]
        public async Task Handle_ExistingEntity_ReturnsTrue()
        {
            var entity = BinMasterBuilders.ValidEntity();
            _mockCmdRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockCmdRepo.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity.Id);

            var result = await CreateSut().Handle(new DeleteBinMasterCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NonExistentEntity_ReturnsFalse()
        {
            _mockCmdRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((WarehouseManagement.Domain.Entities.BinMaster?)null);

            var result = await CreateSut().Handle(new DeleteBinMasterCommand { Id = 999 }, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_AlreadyDeletedEntity_ReturnsFalse()
        {
            var entity = BinMasterBuilders.ValidEntity();
            entity.IsDeleted = BaseEntity.IsDelete.Deleted;
            _mockCmdRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

            var result = await CreateSut().Handle(new DeleteBinMasterCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
