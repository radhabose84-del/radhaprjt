using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.UsageType;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.UsageType
{
    public sealed class CreateUsageTypeCommandValidatorTests
    {
        private readonly Mock<IUsageTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateUsageTypeCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string code = "UTY001", int moduleId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, null))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.ModuleExistsAsync(moduleId))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = UsageTypeBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = UsageTypeBuilders.ValidCreateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("UTY001", null))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.ModuleExistsAsync(1))
                .ReturnsAsync(true);
            var command = UsageTypeBuilders.ValidCreateCommand(code: "UTY001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
