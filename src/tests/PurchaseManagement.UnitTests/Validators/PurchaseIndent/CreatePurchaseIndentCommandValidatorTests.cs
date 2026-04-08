using FluentValidation.TestHelper;
using Contracts.Interfaces.Lookups.Workflow;
using PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent;
using PurchaseManagement.Presentation.Validation.PurchaseIndent;

namespace PurchaseManagement.UnitTests.Validators.PurchaseIndent
{
    public sealed class CreatePurchaseIndentCommandValidatorTests
    {
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);

        private CreatePurchaseIndentCommandValidator CreateValidator() =>
            new(_mockWorkflowLookup.Object);

        [Fact]
        public async Task Validate_EmptyIndentDate_FailsValidation()
        {
            var command = new CreatePurchaseIndentCommand
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
            var command = new CreatePurchaseIndentCommand
            {
                IndentDate = DateOnly.FromDateTime(DateTime.Today),
                IndentTypeId = 1,
                UnitId = 1,
                DepartmentId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IndentDate);
            result.ShouldNotHaveValidationErrorFor(x => x.IndentTypeId);
        }
    }
}
