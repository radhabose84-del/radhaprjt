using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.ItemSpecificationValue;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.ItemSpecificationValue
{
    public sealed class CreateItemSpecificationValueCommandValidatorTests
    {
        private readonly Mock<IItemSpecificationValueQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateItemSpecificationValueCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int specificationMasterId = 1, string value = "Red")
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(specificationMasterId, value, null))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.SpecificationMasterExistsAsync(specificationMasterId))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = ItemSpecificationValueBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyValue_FailsValidation(string? value)
        {
            SetupAllAsyncMocks(value: value!);
            var command = ItemSpecificationValueBuilders.ValidCreateCommand(value: value!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroSpecificationMasterId_FailsValidation()
        {
            SetupAllAsyncMocks(specificationMasterId: 0);
            var command = ItemSpecificationValueBuilders.ValidCreateCommand(specificationMasterId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NonExistentSpecificationMaster_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(99, "Red", null))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.SpecificationMasterExistsAsync(99))
                .ReturnsAsync(false);
            var command = ItemSpecificationValueBuilders.ValidCreateCommand(specificationMasterId: 99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCompositeKey_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(1, "Red", null))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.SpecificationMasterExistsAsync(1))
                .ReturnsAsync(true);
            var command = ItemSpecificationValueBuilders.ValidCreateCommand(specificationMasterId: 1, value: "Red");

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
