using FluentValidation.TestHelper;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetAll;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.BillEntry
{
    public sealed class GetPurchaseBillEntryListQueryValidatorTests
    {
        private GetPurchaseBillEntryListQueryValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ZeroPage_FailsValidation()
        {
            var query = new GetAllPurchaseBillEntryQuery { Page = 0, Size = 10 };

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldHaveValidationErrorFor(x => x.Page);
        }

        [Fact]
        public async Task Validate_SizeOver100_FailsValidation()
        {
            var query = new GetAllPurchaseBillEntryQuery { Page = 1, Size = 101 };

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldHaveValidationErrorFor(x => x.Size);
        }

        [Fact]
        public async Task Validate_ValidQuery_PassesValidation()
        {
            var query = new GetAllPurchaseBillEntryQuery { Page = 1, Size = 15 };

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
