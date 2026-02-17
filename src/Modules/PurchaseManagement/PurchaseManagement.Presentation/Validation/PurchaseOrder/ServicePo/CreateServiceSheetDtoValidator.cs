using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Interfaces.External.IWorkflow;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using FluentValidation;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;
using Contracts.Interfaces.Lookups.Workflow;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo
{
    public class CreateServiceSheetDtoValidator : AbstractValidator<CreateServiceSheetDto>
    {

        private readonly IWorkflowLookup _workflowLookup;            
        private readonly List<ValidationRule> _validationRules;
        public CreateServiceSheetDtoValidator( IWorkflowLookup workflowLookup , List<ValidationRule> validationRules)
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
                                    PurchaseManagement.Domain.Common.MiscEnumEntity.ServiceEntrySheet, // entity type
                                    unitId,
                                    0))                      // DepartmentId not required, pass null
                            .WithMessage(rule.Error);
                        break;
                }
            }

            RuleFor(x => x.SESDate).NotEmpty();
            RuleFor(x => x.SESStatusId).GreaterThan(0);
            RuleFor(x => x.PurchaseOrderId).GreaterThan(0);
            RuleFor(x => x.UnitId).GreaterThan(0);

            RuleFor(x => x.ActualQuantity).GreaterThan(0).When(x => x.ActualQuantity.HasValue);
            RuleFor(x => x.ActualRate).GreaterThanOrEqualTo(0).When(x => x.ActualRate.HasValue);

            RuleFor(x => x.ValidityTo)
                .GreaterThanOrEqualTo(x => x.ValidityFrom!.Value)
                .When(x => x.ValidityFrom.HasValue && x.ValidityTo.HasValue);

            RuleForEach(x => x.Activities).SetValidator(new CreateServiceEntryActivityDtoValidator());
        }
         
          private sealed class CreateServiceEntryActivityDtoValidator : AbstractValidator<CreateServiceSheetDto.CreateServiceEntryActivityDto>
            {
                public CreateServiceEntryActivityDtoValidator()
                {
                    RuleFor(x => x.PerformedById).GreaterThan(0);
                    RuleFor(x => x.Description).MaximumLength(1000);
                    RuleFor(x => x.StatusRemarks).MaximumLength(1000);
                }
            }
        
    }
}