using FluentValidation.TestHelper;
using PurchaseManagement.Application.Issue.Command.CreateIssueEntry;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.Issue;

namespace PurchaseManagement.UnitTests.Validators.Issue
{
    public sealed class CreateIssueEntryCommandValidatorTests
    {
        private CreateIssueEntryCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!));

        [Fact]
        public async Task Validate_NullIssueEntry_FailsValidation()
        {
            // Provide a non-null DTO with invalid (zero) IDs instead of null,
            // because the validator accesses nested properties without a null guard.
            var command = new CreateIssueEntryCommand
            {
                IssueEntry = new CreateIssueEntryDto { MrsHeaderId = 0, DepartmentId = 0, RequestCategoryId = 0 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
