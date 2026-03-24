using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.UsageType;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.UsageType
{
    public sealed class UpdateUsageTypeCommandValidatorTests
    {
        private readonly Mock<IUsageTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateUsageTypeCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, int moduleId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.ModuleExistsAsync(moduleId))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = UsageTypeBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            SetupAllAsyncMocks();
            var command = UsageTypeBuilders.ValidUpdateCommand(name: name!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
