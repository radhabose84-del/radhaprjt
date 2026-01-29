using MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.WorkOrder
{
    public class CreateWorkOrderCommandValidator : AbstractValidator<CreateWorkOrderCommand>
    {
         private readonly List<ValidationRule> _validationRules;

        public CreateWorkOrderCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            // Get max lengths dynamically using MaxLengthProvider
            var woRemarksMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder>("Remarks")??1000;
            var woItemMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderItem>("ItemName")??250;                        
            var woTechnicianMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderTechnician>("TechnicianName")??100;  
            var woActivityMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderActivity>("Description")??1000; 
            var woCheckListMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderCheckList>("Description")??1000; 

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules is null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            // Loop through the rules and apply them
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":       
                        RuleFor(x => x.WorkOrderDto)
                            .Must(x =>
                                (x.RequestId.HasValue && !x.PreventiveScheduleId.HasValue) ||
                                (!x.RequestId.HasValue && x.PreventiveScheduleId.HasValue))
                            .WithMessage("Either RequestId or PreventiveScheduleId must be provided, not both.");                                                                                            
                        //Activity
                        RuleForEach(x => x.WorkOrderDto.WorkOrderActivity)
                            .ChildRules(woActivity =>
                            {
                                woActivity.RuleFor(x => x.ActivityId)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(WorkOrderActivityDto.ActivityId)} {rule.Error}");                                                                   
                        });                         
                        break;
                    case "MaxLength":                                              
                        RuleFor(x => x.WorkOrderDto.Remarks)
                            .MaximumLength(woRemarksMaxLength) 
                            .WithMessage($"{nameof(CreateWorkOrderCommand.WorkOrderDto.Remarks)} {rule.Error} {woRemarksMaxLength}");                                                                            
                        //Item
                        RuleForEach(x => x.WorkOrderDto.WorkOrderItem)
                            .ChildRules(woItem =>
                            {
                                woItem.RuleFor(x => x.ItemName)
                                    .MaximumLength(woItemMaxLength)
                                .WithMessage($"{nameof(WorkOrderItemDto.ItemName)} {rule.Error}{woItemMaxLength}");                              
                            });                   
                          //Activity
                        RuleForEach(x => x.WorkOrderDto.WorkOrderActivity)
                            .ChildRules(woActivity =>
                            {
                                woActivity.RuleFor(x => x.Description)
                                    .MaximumLength(woActivityMaxLength)
                                .WithMessage($"{nameof(WorkOrderActivityDto.Description)} {rule.Error}{woActivityMaxLength}");                              
                            });      
                         //CheckList
                        RuleForEach(x => x.WorkOrderDto.WorkOrderCheckList)
                            .ChildRules(woCheckList =>
                            {
                                woCheckList.RuleFor(x => x.Description)
                                    .MaximumLength(woActivityMaxLength)
                                .WithMessage($"{nameof(WorkOrderCheckListDto.Description)} {rule.Error}{woCheckListMaxLength}");                              
                            });                       
                        break;                          
                }
            }  
        }
    }
}