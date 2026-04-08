using FluentValidation.TestHelper;
using Contracts.Interfaces.Lookups.Workflow;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo;
using Shared.Validation.Common;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.ServicePo
{
    public sealed class CreateServiceSheetDtoValidatorTests
    {
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);

        private CreateServiceSheetDtoValidator CreateValidator()
        {
            var rules = ValidationRuleLoader.LoadValidationRules();
            return new(_mockWorkflowLookup.Object, rules);
        }

        [Fact]
        public async Task Validate_EmptyDto_FailsValidation()
        {
            var dto = new CreateServiceSheetDto();

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
