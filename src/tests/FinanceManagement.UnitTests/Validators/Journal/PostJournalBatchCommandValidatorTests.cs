using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournalBatch;
using FinanceManagement.Presentation.Validation.JournalMaster.Journal;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.Journal
{
    public sealed class PostJournalBatchCommandValidatorTests
    {
        private readonly PostJournalBatchCommandValidator _validator = new();

        [Fact]
        public async Task Validate_ValidIds_Passes()
        {
            var result = await _validator.TestValidateAsync(new PostJournalBatchCommand { Ids = new() { 3, 4, 5 } });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyIds_Fails()
        {
            var result = await _validator.TestValidateAsync(new PostJournalBatchCommand { Ids = new() });
            result.ShouldHaveValidationErrorFor(x => x.Ids);
        }

        [Fact]
        public async Task Validate_NonPositiveId_Fails()
        {
            var result = await _validator.TestValidateAsync(new PostJournalBatchCommand { Ids = new() { 3, 0 } });
            result.IsValid.Should().BeFalse();
        }
    }
}
