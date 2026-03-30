using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.Item.ItemGroup;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.ItemGroup
{
    public sealed class UpdateItemGroupCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IItemGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        public UpdateItemGroupCommandValidatorTests()
        {
            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.IsCodeDuplicateAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(ItemGroupBuilders.ValidDto());
        }

        private UpdateItemGroupCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockCommandRepo.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ItemGroupBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = ItemGroupBuilders.ValidUpdateCommand(name: name!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = ItemGroupBuilders.ValidUpdateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            var command = ItemGroupBuilders.ValidUpdateCommand(name: "Duplicate Group");

            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync("Duplicate Group", command.Id))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = ItemGroupBuilders.ValidUpdateCommand(code: "DUPCODE");

            _mockCommandRepo
                .Setup(r => r.IsCodeDuplicateAsync("DUPCODE", command.Id))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
