using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Commands.DeleteItemSpecificationMaster;
using InventoryManagement.Presentation.Validation.ItemSpecificationMaster;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.ItemSpecificationMaster
{
    public sealed class DeleteItemSpecificationMasterCommandValidatorTests
    {
        private readonly Mock<IItemSpecificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteItemSpecificationMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(false);
            var command = ItemSpecificationMasterBuilders.ValidDeleteCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteItemSpecificationMasterCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = new DeleteItemSpecificationMasterCommand(99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_LinkedRecord_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);
            var command = ItemSpecificationMasterBuilders.ValidDeleteCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("linked with other records", StringComparison.OrdinalIgnoreCase));
        }
    }
}
