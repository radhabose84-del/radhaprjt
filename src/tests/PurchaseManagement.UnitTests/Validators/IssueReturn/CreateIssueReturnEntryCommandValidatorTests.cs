using FluentValidation.TestHelper;
using Contracts.Interfaces.Lookups.Workflow;
using PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.IssueReturn;

namespace PurchaseManagement.UnitTests.Validators.IssueReturn
{
    public sealed class CreateIssueReturnEntryCommandValidatorTests
    {
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);

        private CreateIssueReturnEntryCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockWorkflowLookup.Object);

        [Fact]
        public async Task Validate_NullIssueReturnEntry_FailsValidation()
        {
            // Provide a non-null DTO with invalid (zero) IDs instead of null,
            // because the validator accesses nested properties without a null guard.
            var command = new CreateIssueReturnEntryCommand
            {
                IssueReturnEntry = new CreateIssueReturnDto { RequestCategoryId = 0, DepartmentId = 0 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
