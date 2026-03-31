using FluentValidation.TestHelper;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Command.DeleteRackMaster;
using WarehouseManagement.Presentation.Validation.RackMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Validators.RackMaster
{
    public sealed class DeleteRackMasterCommandValidatorTests
    {
        private readonly Mock<IRackMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        private DeleteRackMasterCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object);

        [Fact]
        public async Task Validate_ValidId_EntityExists_PassesValidation()
        {
            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(RackMasterBuilders.ValidEntity(1));

            var result = await CreateValidator()
                .TestValidateAsync(new DeleteRackMasterCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator()
                .TestValidateAsync(new DeleteRackMasterCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_EntityNotFound_FailsWithCorrectMessage()
        {
            _mockCommandRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((WarehouseManagement.Domain.Entities.RackMaster?)null);

            var result = await CreateValidator()
                .TestValidateAsync(new DeleteRackMasterCommand { Id = 99 });

            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("RackMaster Id 99 not found.");
        }
    }
}
