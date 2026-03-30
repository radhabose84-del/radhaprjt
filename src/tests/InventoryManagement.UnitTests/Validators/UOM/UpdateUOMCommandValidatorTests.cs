using FluentValidation.TestHelper;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.UOM;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.UOM
{
    public sealed class UpdateUOMCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateUOMCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = UOMBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = UOMBuilders.ValidUpdateCommand(name: name!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
