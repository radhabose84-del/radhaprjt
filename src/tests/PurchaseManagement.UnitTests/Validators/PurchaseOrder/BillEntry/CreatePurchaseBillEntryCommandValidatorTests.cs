using FluentValidation.TestHelper;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Create;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.BillEntry;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.BillEntry
{
    public sealed class CreatePurchaseBillEntryCommandValidatorTests
    {
        private CreatePurchaseBillEntryCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_NullData_FailsValidation()
        {
            var command = new CreatePurchaseBillEntryCommand(null!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Data);
        }

        [Fact]
        public async Task Validate_ValidData_PassesValidation()
        {
            var command = new CreatePurchaseBillEntryCommand(new PurchaseBillEntryHeaderDto
            {
                UnitId = 1
            });

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Data);
        }
    }
}
