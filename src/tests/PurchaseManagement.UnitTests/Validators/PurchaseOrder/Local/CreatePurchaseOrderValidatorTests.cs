using FluentValidation.TestHelper;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.Local;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.Local
{
    public sealed class CreatePurchaseOrderValidatorTests
    {
        private CreatePurchaseOrderValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ZeroVendorId_FailsValidation()
        {
            var dto = new PurchaseOrderCreateDto { VendorId = 0, CurrencyId = 1 };

            var result = await CreateValidator().TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.VendorId);
        }

        [Fact]
        public async Task Validate_NullHeaders_FailsValidation()
        {
            // Provide an empty list instead of null, because the validator's
            // .Must(h => h!.Count > 0) throws NullReferenceException on null.
            var dto = new PurchaseOrderCreateDto
            {
                VendorId = 1,
                CurrencyId = 1,
                Headers = new List<PurchaseLocalHeaderDto>()
            };

            var result = await CreateValidator().TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.Headers);
        }
    }
}
