using FluentValidation.TestHelper;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.DeleteWarehouseMaster;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Presentation.Validation.BinMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Validators.BinMaster
{
    public sealed class DeleteBinMasterCommandValidatorTests
    {
        private readonly Mock<IBinMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        private DeleteBinMasterCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object);

        [Fact]
        public async Task Validate_ValidId_EntityExists_PassesValidation()
        {
            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BinMasterBuilders.ValidEntity(1));

            var result = await CreateValidator()
                .TestValidateAsync(new DeleteWarehouseMasterCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator()
                .TestValidateAsync(new DeleteWarehouseMasterCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_EntityNotFound_FailsWithCorrectMessage()
        {
            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((WarehouseManagement.Domain.Entities.BinMaster?)null);

            var result = await CreateValidator()
                .TestValidateAsync(new DeleteWarehouseMasterCommand { Id = 99 });

            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("Bin not found or already deleted.");
        }

        [Fact]
        public async Task Validate_AlreadyDeletedEntity_FailsWithCorrectMessage()
        {
            var deletedEntity = BinMasterBuilders.ValidEntity(1);
            deletedEntity.IsDeleted = BaseEntity.IsDelete.Deleted;

            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedEntity);

            var result = await CreateValidator()
                .TestValidateAsync(new DeleteWarehouseMasterCommand { Id = 1 });

            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("Bin not found or already deleted.");
        }
    }
}
