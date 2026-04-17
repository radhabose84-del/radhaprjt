using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.DeletePriceGroupMaster;
using InventoryManagement.Presentation.Validation.PriceGroupMaster;

namespace InventoryManagement.UnitTests.Validators.PriceGroupMaster
{
    public sealed class DeletePriceGroupMasterCommandValidatorTests
    {
        private readonly Mock<IPriceGroupMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeletePriceGroupMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(5)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeletePriceGroupMasterCommand(5));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeletePriceGroupMasterCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeletePriceGroupMasterCommand(99));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
