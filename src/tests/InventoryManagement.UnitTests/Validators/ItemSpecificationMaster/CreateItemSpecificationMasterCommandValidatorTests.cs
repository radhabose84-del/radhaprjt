using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.ItemSpecificationMaster;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.ItemSpecificationMaster
{
    public sealed class CreateItemSpecificationMasterCommandValidatorTests
    {
        private readonly Mock<IItemSpecificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateItemSpecificationMasterCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string code = "SPEC001", string name = "Color", int order = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, null))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NameAlreadyExistsAsync(name, null))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.OrderAlreadyExistsAsync(order, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = ItemSpecificationMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            SetupAllAsyncMocks();
            var command = ItemSpecificationMasterBuilders.ValidCreateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            SetupAllAsyncMocks();
            var command = ItemSpecificationMasterBuilders.ValidCreateCommand(name: name!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("SPEC001", null))
                .ReturnsAsync(true);
            var command = ItemSpecificationMasterBuilders.ValidCreateCommand(code: "SPEC001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NameAlreadyExistsAsync("Color", null))
                .ReturnsAsync(true);
            var command = ItemSpecificationMasterBuilders.ValidCreateCommand(name: "Color");

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateOrder_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.OrderAlreadyExistsAsync(1, null))
                .ReturnsAsync(true);
            var command = ItemSpecificationMasterBuilders.ValidCreateCommand(order: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroOrder_FailsValidation()
        {
            SetupAllAsyncMocks(order: 0);
            var command = ItemSpecificationMasterBuilders.ValidCreateCommand(order: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("SPEC-01")]
        [InlineData("SPEC 01")]
        [InlineData("SPEC@01")]
        public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
        {
            SetupAllAsyncMocks(code: code);
            var command = ItemSpecificationMasterBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
