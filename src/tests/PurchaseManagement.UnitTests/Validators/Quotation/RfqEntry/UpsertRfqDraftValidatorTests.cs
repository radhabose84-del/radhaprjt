using FluentValidation.TestHelper;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.UpsertDraft;
using PurchaseManagement.Presentation.Validation.Quotation.RfqEntry;

namespace PurchaseManagement.UnitTests.Validators.Quotation.RfqEntry
{
    public sealed class UpsertRfqDraftValidatorTests
    {
        private UpsertRfqDraftValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_NegativeId_FailsValidation()
        {
            var command = new UpsertRfqDraftCommand { Id = -1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NullId_PassesValidation()
        {
            var command = new UpsertRfqDraftCommand { Id = null };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_PositiveId_PassesValidation()
        {
            var command = new UpsertRfqDraftCommand { Id = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
