using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Presentation.Validation.MaintenanceCategory;
using MaintenanceManagement.UnitTests.TestData;

namespace MaintenanceManagement.UnitTests.Validators.MaintenanceCategory
{
    public sealed class DeleteMaintenanceCategoryCommandValidatorTests
    {
        private readonly Mock<IMaintenanceCategoryQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteMaintenanceCategoryCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MaintenanceCategoryBuilders.ValidDeleteCommand(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(MaintenanceCategoryBuilders.ValidEntity(1));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = MaintenanceCategoryBuilders.ValidDeleteCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_RecordNotFound_FailsValidation()
        {
            var command = MaintenanceCategoryBuilders.ValidDeleteCommand(999);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((MaintenanceManagement.Domain.Entities.MaintenanceCategory?)null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
