using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.ItemSpecificationMaster;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.ItemSpecificationMaster
{
    public sealed class UpdateItemSpecificationMasterCommandValidatorTests
    {
        private readonly Mock<IItemSpecificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateItemSpecificationMasterCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, string name = "Updated Color", int order = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NameAlreadyExistsAsync(name, id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.OrderAlreadyExistsAsync(order, id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.IsItemSpecificationMasterLinkedAsync(id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = ItemSpecificationMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            SetupAllAsyncMocks(name: name!);
            var command = ItemSpecificationMasterBuilders.ValidUpdateCommand(name: name!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = ItemSpecificationMasterBuilders.ValidUpdateCommand(id: 99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NameAlreadyExistsAsync("Updated Color", 1))
                .ReturnsAsync(true);
            var command = ItemSpecificationMasterBuilders.ValidUpdateCommand(name: "Updated Color");

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateOrder_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.OrderAlreadyExistsAsync(1, 1))
                .ReturnsAsync(true);
            var command = ItemSpecificationMasterBuilders.ValidUpdateCommand(order: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroOrder_FailsValidation()
        {
            SetupAllAsyncMocks(order: 0);
            var command = ItemSpecificationMasterBuilders.ValidUpdateCommand(order: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
        {
            SetupAllAsyncMocks();
            var command = ItemSpecificationMasterBuilders.ValidUpdateCommand(isActive: isActive);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_InactivateWhenLinked_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NameAlreadyExistsAsync("Updated Color", 1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.OrderAlreadyExistsAsync(1, 1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsItemSpecificationMasterLinkedAsync(1)).ReturnsAsync(true);

            var command = ItemSpecificationMasterBuilders.ValidUpdateCommand(isActive: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("linked with other records", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Validate_InactivateWhenNotLinked_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = ItemSpecificationMasterBuilders.ValidUpdateCommand(isActive: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
