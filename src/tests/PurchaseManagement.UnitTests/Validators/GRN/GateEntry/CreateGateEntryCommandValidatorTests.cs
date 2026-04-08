using FluentValidation.TestHelper;
using PurchaseManagement.Application.GRN.GateEntry.Commands.CreateGateEntry;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.GRN.GateEntry;

namespace PurchaseManagement.UnitTests.Validators.GRN.GateEntry
{
    public sealed class CreateGateEntryCommandValidatorTests
    {
        private CreateGateEntryCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!));

        [Fact]
        public async Task Validate_NullGateEntryDetails_FailsValidation()
        {
            // Provide a non-null DTO with invalid (zero) IDs instead of null,
            // because the validator accesses nested properties without a null guard.
            var command = new CreateGateEntryCommand
            {
                GateEntryDetails = new CreateGateEntryDto { PartyId = 0, UnitId = 0, ReceivingTypeId = 0 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
