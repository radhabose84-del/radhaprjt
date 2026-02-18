using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using FluentValidation;
using Shared.Validation.Common;
using Contracts.Interfaces.Lookups.Workflow;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo
{
    public class CreateServicePurchaseOrderValidator : AbstractValidator<CreateServicePurchaseOrderDto>
    {

        private readonly IWorkflowLookup _workflowLookup;      
        private readonly List<ValidationRule> _validationRules;
       
         
        public CreateServicePurchaseOrderValidator(IWorkflowLookup workflowLookup , List<ValidationRule> validationRules , IServicePurchaseOrderCommandRepository servicePurchaseOrderCommandRepository)
        {
            _workflowLookup = workflowLookup;
            _validationRules = validationRules;

            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
             foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {

                   case "Workflow":
                                    RuleFor(x => x.UnitId)
                                .MustAsync(async (unitId, cancellation) =>
                                    await _workflowLookup.IsApproveWorkflowConfigureAsync(
                                        PurchaseManagement.Domain.Common.MiscEnumEntity.ServicePO, // entity type
                                        unitId,
                                        0))                      // DepartmentId not required, pass null
                                .WithMessage(rule.Error);
                            break;
                }
            }

            RuleFor(x => x.UnitId).GreaterThan(0);
            RuleFor(x => x.PODate).NotEmpty();
            RuleFor(x => x.VendorId).GreaterThan(0);
            RuleForEach(x => x.ServicePos).SetValidator(new ServiceHeaderValidator());
        }

        private class ServiceHeaderValidator : AbstractValidator<PurchaseOrderServiceHeaderDto>
        {
            public ServiceHeaderValidator()
            {
                RuleFor(x => x.ServiceCategoryId).GreaterThan(0);
                RuleForEach(x => x.Lines).SetValidator(new ServiceLineValidator());
            }
        }

        private class ServiceLineValidator : AbstractValidator<PurchaseOrderServiceLineDto>
        {
            public ServiceLineValidator()
            {
                RuleFor(x => x.PlannedQuantity).GreaterThan(0);
                RuleFor(x => x.PlannedRate).GreaterThan(0);
            }
        }
    }
}