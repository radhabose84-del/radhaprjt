using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler;
using MaintenanceManagement.Domain.Common;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.PreventiveSchedulers
{
    public class UpdatePreventiveSchedulerCommandValidator : AbstractValidator<UpdatePreventiveSchedulerCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMachineGroupQueryRepository _machineGroupQueryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IActivityMasterQueryRepository _activityMasterQueryRepository;
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        public UpdatePreventiveSchedulerCommandValidator(IMachineGroupQueryRepository machineGroupQueryRepository,IMiscMasterQueryRepository miscMasterQueryRepository,IActivityMasterQueryRepository activityMasterQueryRepository,IPreventiveSchedulerQuery preventiveSchedulerQuery)
        {
             _validationRules = ValidationRuleLoader.LoadValidationRules();
            _machineGroupQueryRepository = machineGroupQueryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _activityMasterQueryRepository = activityMasterQueryRepository;
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
              if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
             foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                     RuleFor(x => x.Id)
                                .NotNull()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.Id)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.Id)} {rule.Error}");
                        RuleFor(x => x.MachineGroupId)
                                .NotNull()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.MachineGroupId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.MachineGroupId)} {rule.Error}");
                        RuleFor(x => x.DepartmentId)
                                .NotNull()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.DepartmentId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.DepartmentId)} {rule.Error}");
                        RuleFor(x => x.MaintenanceCategoryId)
                                .NotNull()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.MaintenanceCategoryId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.MaintenanceCategoryId)} {rule.Error}");
                        RuleFor(x => x.ScheduleId)
                                .NotNull()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.ScheduleId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.ScheduleId)} {rule.Error}");
                        RuleFor(x => x.FrequencyTypeId)
                                .NotNull()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.FrequencyTypeId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.FrequencyTypeId)} {rule.Error}");
                        RuleFor(x => x.FrequencyInterval)
                                .NotNull()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.FrequencyInterval)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.FrequencyInterval)} {rule.Error}");
                        RuleFor(x => x.FrequencyUnitId)
                                .NotNull()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.FrequencyUnitId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.FrequencyUnitId)} {rule.Error}");
                        RuleFor(x => x.EffectiveDate)
                                .NotNull()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.EffectiveDate)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.EffectiveDate)} {rule.Error}");

                         RuleFor(x => x.GraceDays)
                                .NotNull()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.GraceDays)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.GraceDays)} {rule.Error}");
                        RuleFor(x => x.ReminderWorkOrderDays)
                                .GreaterThanOrEqualTo(0)
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.ReminderWorkOrderDays)} {rule.Error}");
                        RuleFor(x => x.ReminderMaterialReqDays)
                                .GreaterThanOrEqualTo(0)
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.ReminderMaterialReqDays)} {rule.Error}");
                        RuleFor(x => x.IsDownTimeRequired)
                                .NotNull()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.IsDownTimeRequired)} {rule.Error}");
                        RuleFor(x => x.DownTimeEstimateHrs)
                                .NotNull()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.DownTimeEstimateHrs)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(UpdatePreventiveSchedulerCommand.DownTimeEstimateHrs)} {rule.Error}")
                                .When(x => x.IsDownTimeRequired == 1);
                        RuleFor(x => x.Activity)
                            .NotNull()
                            .WithMessage($"{rule.Error}")
                            .Must(x => x.Count > 0)
                            .WithMessage($"{rule.Error}");
                        RuleFor(x => x)
                                .Must(x => x.ReminderWorkOrderDays < x.FrequencyInterval)
                           .WithMessage("Work Reminder Days should be less than Frequency.");
                        RuleFor(x => x)
                                .Must(x => x.ReminderMaterialReqDays < x.FrequencyInterval)
                           .WithMessage("Material Reminder Days should be less than Frequency.");
                    break;
                case "FKColumnDelete":
                        RuleFor(x => x.MachineGroupId)
                               .MustAsync(async (MachineGroupId, cancellation) => 
                                 await _machineGroupQueryRepository.FKColumnExistValidation(MachineGroupId))
                                .WithMessage($"{rule.Error}");  
                        RuleFor(x => x.MaintenanceCategoryId)
                               .MustAsync(async (MaintenanceCategoryId, cancellation) => 
                                 await _miscMasterQueryRepository.FKColumnValidation(MaintenanceCategoryId))
                                .WithMessage($"{rule.Error}");  
                        RuleFor(x => x.ScheduleId)
                               .MustAsync(async (ScheduleId, cancellation) => 
                                 await _miscMasterQueryRepository.FKColumnValidation(ScheduleId))
                                .WithMessage($"{rule.Error}");  
                        RuleFor(x => x.FrequencyTypeId)
                               .MustAsync(async (FrequencyTypeId, cancellation) => 
                                 await _miscMasterQueryRepository.FKColumnValidation(FrequencyTypeId))
                                .WithMessage($"{rule.Error}"); 
                        RuleFor(x => x.FrequencyUnitId)
                               .MustAsync(async (FrequencyUnitId, cancellation) => 
                                 await _miscMasterQueryRepository.FKColumnValidation(FrequencyUnitId))
                                .WithMessage($"{rule.Error}");   
                        RuleFor(x => x.Activity)
                          .ForEach(activityRule =>
                          {
                              activityRule.MustAsync(async (activity, cancellation) => 
                                  await _activityMasterQueryRepository.FKColumnExistValidation(activity.ActivityId))
                                  .WithMessage($"{rule.Error}");  
                          }); 

                    break;
                //     case "DateValidation":
                //      RuleFor(x => x.EffectiveDate)
                //                 .Must(BeAValidDate)
                //                 .WithMessage($"{rule.Error}"); 
                //     break;
                //     case "PastDateValidation":
                //      RuleFor(x => x.EffectiveDate)
                //                 .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                //                 .WithMessage($"{rule.Error}"); 
                //     break;
                    case "NotFound":
                     RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _preventiveSchedulerQuery.NotFoundAsync(Id))
                            .WithMessage($"{rule.Error}");
                            break;  
                //   case "WorkOrderValidate":
                //      RuleFor(x => x.Id )
                //            .MustAsync(async (Id, cancellation) => 
               //  //         !await _preventiveSchedulerQuery.UpdateValidation(Id))
                //             .WithMessage($"{rule.Error}");
                //             break;  
                  case "AlreadyExists":
                        RuleForEach(x => x.Activity)
                         .MustAsync(async (command, activityDto, context, cancellation) =>
                         {
                             return !await _preventiveSchedulerQuery.AlreadyExistsAsync(
                                 activityDto.ActivityId, 
                                 command.MachineGroupId,        
                                 command.Id                     
                             );
                         }) 
                         .WithMessage($"{rule.Error}");  
                         
                            break;  
                            case "MachineGroupValidation":
                                RuleFor(x => x.MachineGroupId)
                                  .MustAsync(async (MachineGroupId, cancellation) => 
                                 await _preventiveSchedulerQuery.MachingroupValidation(MachineGroupId))
                                .WithMessage($"{rule.Error}");

                                RuleFor(x => x.Id)
                                .MustAsync(async (command,id, context,cancellation) =>
                                 {
                                         var current = await _preventiveSchedulerQuery
                                                .OnetimeFrequencyValidation(id, cancellation);

                                                var incoming = command.FrequencyTypeId;
                                                var incomingFrequency = await _miscMasterQueryRepository.GetByIdAsync(incoming);
                                               if (current.FrequencyTypeId == incoming) return true;

                                         var isFlip =
                                          (current.MiscFrequencyType.Code == MiscEnumEntity.FrequencyType.Code && incomingFrequency.Code == MiscEnumEntity.Every)
                                          || (current.MiscFrequencyType.Code == MiscEnumEntity.Every && incomingFrequency.Code == MiscEnumEntity.FrequencyType.Code);
                                                 if (!isFlip) return true;
                                                
                
                                   return !await _preventiveSchedulerQuery.OneTimeSchedulerValidate(
                                                                                     id,
                                                                                     cancellation); 
                                  })                
                               .WithMessage("Work Order has already been generated. You cannot update frequency type."); 
                                break;
                    default:                        
                        break;   
                }
            }
        }
         
    }
}