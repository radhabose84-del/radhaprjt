using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MaintenanceCategory;
using MaintenanceManagement.UnitTests.TestData;

namespace MaintenanceManagement.UnitTests.Validators.MaintenanceCategory
{
    public sealed class CreateMaintenanceCategoryCommandValidatorTests
    {
        private readonly Mock<IMaintenanceCategoryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateMaintenanceCategoryCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockMaxLength.Object);

        private void SetupAllAsyncMocks(string categoryName = "Electrical")
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(categoryName))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MaintenanceCategoryBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.CategoryName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCategoryName_FailsValidation(string? categoryName)
        {
            var command = MaintenanceCategoryBuilders.ValidCreateCommand(categoryName: categoryName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCategoryName_FailsValidation()
        {
            var command = MaintenanceCategoryBuilders.ValidCreateCommand(categoryName: "Electrical");
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync("Electrical"))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
