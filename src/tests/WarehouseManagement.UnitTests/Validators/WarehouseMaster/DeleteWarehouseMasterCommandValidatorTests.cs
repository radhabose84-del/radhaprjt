using FluentValidation.TestHelper;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.DeleteWarehouseMaster;
using WarehouseManagement.Presentation.Validation.WarehouseMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Validators.WarehouseMaster
{
    public sealed class DeleteWarehouseMasterCommandValidatorTests
    {
        private readonly Mock<IWarehouseMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        private DeleteWareMasterCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object);

        [Fact]
        public async Task Validate_ValidId_EntityExists_PassesValidation()
        {
            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(WarehouseMasterBuilders.ValidEntity(1));

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
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((WarehouseManagement.Domain.Entities.WarehouseMaster?)null);

            var result = await CreateValidator()
                .TestValidateAsync(new DeleteWarehouseMasterCommand { Id = 99 });

            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("WarehouseMaster Id 99 not found.");
        }
    }
}
