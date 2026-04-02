using Contracts.Interfaces.Lookups.Workflow;
using FluentValidation.TestHelper;
using InventoryManagement.Application.MRS.Command.CreateMrsEntry;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.MRS;
using static InventoryManagement.Application.MRS.Command.CreateMrsEntry.CreateMrsEntryDto;

namespace InventoryManagement.UnitTests.Validators.MRS
{
    public sealed class CreateMrsEntryCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IWorkflowLookup> _mockWorkflow = new(MockBehavior.Loose);

        public CreateMrsEntryCommandValidatorTests()
        {
            _mockWorkflow
                .Setup(r => r.IsApproveWorkflowConfigureAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);
        }

        private CreateMrsEntryCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockWorkflow.Object);

        private static CreateMrsEntryCommand ValidCommand() => new()
        {
            MrsEntry = new CreateMrsEntryDto
            {
                UnitId = 1,
                RequestCategoryId = 1,
                DepartmentId = 1,
                SubDepartmentId = 1,
                MrsDetails = new List<CreateMrsDetailDto>
                {
                    new CreateMrsDetailDto { ItemId = 1, UomId = 1, RequestQuantity = 5m }
                }
            }
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroRequestCategoryId_FailsValidation()
        {
            var command = ValidCommand();
            command.MrsEntry.RequestCategoryId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroDepartmentId_FailsValidation()
        {
            var command = ValidCommand();
            command.MrsEntry.DepartmentId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroSubDepartmentId_FailsValidation()
        {
            var command = ValidCommand();
            command.MrsEntry.SubDepartmentId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DetailWithZeroItemId_FailsValidation()
        {
            var command = ValidCommand();
            command.MrsEntry.MrsDetails[0].ItemId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_WorkflowNotConfigured_NoWorkflowRuleInJson()
        {
            // "Workflow" rule is not in validation-rules.json, so no workflow
            // validation is registered — the validator does not reject this case.
            _mockWorkflow
                .Setup(r => r.IsApproveWorkflowConfigureAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
