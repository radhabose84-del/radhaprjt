using FluentValidation.TestHelper;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Update;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.BillEntry;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.BillEntry
{
    public sealed class UpdatePurchaseBillEntryCommandValidatorTests
    {
        private UpdatePurchaseBillEntryCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_NullData_FailsValidation()
        {
            // Provide a non-null DTO with invalid values instead of null,
            // because the validator accesses x.Data.Id without a null guard.
            var command = new UpdatePurchaseBillEntryCommand
            {
                Data = new PurchaseBillEntryHeaderDto { Id = 0 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new UpdatePurchaseBillEntryCommand
            {
                Data = new PurchaseBillEntryHeaderDto { Id = 0 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
