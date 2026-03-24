using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MaintenanceCategory;
using MaintenanceManagement.UnitTests.TestData;

namespace MaintenanceManagement.UnitTests.Validators.MaintenanceCategory
{
    public sealed class UpdateMaintenanceCategoryCommandValidatorTests
    {
        private readonly Mock<IMaintenanceCategoryCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMaintenanceCategoryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateMaintenanceCategoryCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1, string categoryName = "Updated Electrical")
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(MaintenanceCategoryBuilders.ValidEntity(id));
            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(categoryName, id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MaintenanceCategoryBuilders.ValidUpdateCommand();
            SetupHappyPath(command.Id, command.CategoryName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCategoryName_FailsValidation(string? categoryName)
        {
            var command = MaintenanceCategoryBuilders.ValidUpdateCommand(categoryName: categoryName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_RecordNotFound_FailsValidation()
        {
            var command = MaintenanceCategoryBuilders.ValidUpdateCommand(id: 999);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((MaintenanceManagement.Domain.Entities.MaintenanceCategory?)null);
            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(command.CategoryName, 999))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
