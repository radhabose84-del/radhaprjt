using FluentValidation.TestHelper;
using Contracts.Interfaces.Lookups.Workflow;
using PurchaseManagement.Application.PurchaseIndents.Command.UpdatePurchaseIndent;
using PurchaseManagement.Presentation.Validation.PurchaseIndent;

namespace PurchaseManagement.UnitTests.Validators.PurchaseIndent
{
    public sealed class UpdatePurchaseIndentCommandValidatorTests
    {
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);

        private UpdatePurchaseIndentCommandValidator CreateValidator() =>
            new(_mockWorkflowLookup.Object);

        [Fact]
        public async Task Validate_EmptyIndentDate_FailsValidation()
        {
            var command = new UpdatePurchaseIndentCommand
            {
                IndentDate = default,
                IndentTypeId = 1,
                UnitId = 1,
                DepartmentId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IndentDate);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesBasicValidation()
        {
            var command = new UpdatePurchaseIndentCommand
            {
                IndentDate = DateOnly.FromDateTime(DateTime.Today),
                IndentTypeId = 1,
                UnitId = 1,
                DepartmentId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IndentDate);
        }
    }
}
