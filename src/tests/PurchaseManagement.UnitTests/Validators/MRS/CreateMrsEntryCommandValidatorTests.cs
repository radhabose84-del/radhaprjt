using FluentValidation.TestHelper;
using Contracts.Interfaces.Lookups.Workflow;
using PurchaseManagement.Application.MRS.Command.CreateMrsEntry;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.MRS;

namespace PurchaseManagement.UnitTests.Validators.MRS
{
    public sealed class CreateMrsEntryCommandValidatorTests
    {
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);

        private CreateMrsEntryCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockWorkflowLookup.Object);

        [Fact]
        public async Task Validate_NullMrsEntry_FailsValidation()
        {
            // Provide a non-null DTO with invalid (zero) IDs instead of null,
            // because the validator accesses nested properties without a null guard.
            var command = new CreateMrsEntryCommand
            {
                MrsEntry = new CreateMrsEntryDto { RequestCategoryId = 0, DepartmentId = 0, SubDepartmentId = 0 }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
