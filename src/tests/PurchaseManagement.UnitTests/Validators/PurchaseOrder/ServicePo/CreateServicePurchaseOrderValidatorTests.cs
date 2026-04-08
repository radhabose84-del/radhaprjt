using FluentValidation.TestHelper;
using Contracts.Interfaces.Lookups.Workflow;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo;
using Shared.Validation.Common;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.ServicePo
{
    public sealed class CreateServicePurchaseOrderValidatorTests
    {
        private readonly Mock<IWorkflowLookup> _mockWorkflowLookup = new(MockBehavior.Loose);
        private readonly Mock<IServicePurchaseOrderCommandRepository> _mockCmdRepo = new(MockBehavior.Loose);

        private CreateServicePurchaseOrderValidator CreateValidator()
        {
            var rules = ValidationRuleLoader.LoadValidationRules();
            return new(_mockWorkflowLookup.Object, rules, _mockCmdRepo.Object);
        }

        [Fact]
        public async Task Validate_EmptyDto_FailsValidation()
        {
            var dto = new CreateServicePurchaseOrderDto();

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
