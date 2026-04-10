using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.ItemSpecificationValue;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.ItemSpecificationValue
{
    public sealed class UpdateItemSpecificationValueCommandValidatorTests
    {
        private readonly Mock<IItemSpecificationValueQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateItemSpecificationValueCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, int specificationMasterId = 1, string value = "Updated Red")
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(specificationMasterId, value, id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.SpecificationMasterExistsAsync(specificationMasterId))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.IsItemSpecificationValueLinkedAsync(id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = ItemSpecificationValueBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyValue_FailsValidation(string? value)
        {
            SetupAllAsyncMocks(value: value!);
            var command = ItemSpecificationValueBuilders.ValidUpdateCommand(value: value!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SpecificationMasterExistsAsync(1)).ReturnsAsync(true);
            var command = ItemSpecificationValueBuilders.ValidUpdateCommand(id: 99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroSpecificationMasterId_FailsValidation()
        {
            SetupAllAsyncMocks(specificationMasterId: 0);
            var command = ItemSpecificationValueBuilders.ValidUpdateCommand(specificationMasterId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NonExistentSpecificationMaster_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SpecificationMasterExistsAsync(99)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsItemSpecificationValueLinkedAsync(1)).ReturnsAsync(false);
            var command = ItemSpecificationValueBuilders.ValidUpdateCommand(specificationMasterId: 99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCompositeKey_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(1, "Updated Red", 1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SpecificationMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsItemSpecificationValueLinkedAsync(1)).ReturnsAsync(false);
            var command = ItemSpecificationValueBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
        {
            SetupAllAsyncMocks();
            var command = ItemSpecificationValueBuilders.ValidUpdateCommand(isActive: isActive);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_InactivateWhenLinked_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(1, "Updated Red", 1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SpecificationMasterExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsItemSpecificationValueLinkedAsync(1)).ReturnsAsync(true);

            var command = ItemSpecificationValueBuilders.ValidUpdateCommand(isActive: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("linked with other records", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Validate_InactivateWhenNotLinked_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = ItemSpecificationValueBuilders.ValidUpdateCommand(isActive: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
