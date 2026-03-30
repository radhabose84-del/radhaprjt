using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.Item.ItemGroup;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.ItemGroup
{
    public sealed class CreateItemGroupCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IItemGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        public CreateItemGroupCommandValidatorTests()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        private CreateItemGroupCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockCommandRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ItemGroupBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = ItemGroupBuilders.ValidCreateCommand(name: name!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = ItemGroupBuilders.ValidCreateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = ItemGroupBuilders.ValidCreateCommand(code: "EXIST01");

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync("EXIST01"))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            var command = ItemGroupBuilders.ValidCreateCommand(name: "Existing Group");

            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync("Existing Group", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
