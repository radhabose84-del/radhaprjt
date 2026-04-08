using FluentValidation.TestHelper;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.Local;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.Local
{
    public sealed class DeletePurchaseOrderValidatorTests
    {
        private DeletePurchaseOrderValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(0);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_PositiveId_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(1);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
