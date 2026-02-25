#nullable disable
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.CreatePreventiveScheduler;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.PreventiveSchedulers
{
    public class CreatePreventiveSchedulerCommandValidator : AbstractValidator<CreatePreventiveSchedulerCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMachineGroupQueryRepository _machineGroupQueryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IActivityMasterQueryRepository _activityMasterQueryRepository;
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        public CreatePreventiveSchedulerCommandValidator(MaxLengthProvider maxLengthProvider, IMachineGroupQueryRepository machineGroupQueryRepository,IMiscMasterQueryRepository miscMasterQueryRepository,IActivityMasterQueryRepository activityMasterQueryRepository,IPreventiveSchedulerQuery preventiveSchedulerQuery)
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
                        RuleFor(x => x.MachineGroupId)
                                .NotNull()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.MachineGroupId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.MachineGroupId)} {rule.Error}");
                        RuleFor(x => x.DepartmentId)
                                .NotNull()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.DepartmentId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.DepartmentId)} {rule.Error}");
                        RuleFor(x => x.MaintenanceCategoryId)
                                .NotNull()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.MachineGroupId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.MachineGroupId)} {rule.Error}");
                        RuleFor(x => x.ScheduleId)
                                .NotNull()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.ScheduleId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.ScheduleId)} {rule.Error}");
                        RuleFor(x => x.FrequencyTypeId)
                                .NotNull()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.FrequencyTypeId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.FrequencyTypeId)} {rule.Error}");
                        RuleFor(x => x.FrequencyInterval)
                                .NotNull()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.FrequencyInterval)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.FrequencyInterval)} {rule.Error}");
                        RuleFor(x => x.FrequencyUnitId)
                                .NotNull()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.FrequencyUnitId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.FrequencyUnitId)} {rule.Error}");
                        RuleFor(x => x.EffectiveDate)
                                .NotNull()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.EffectiveDate)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.EffectiveDate)} {rule.Error}");

                         RuleFor(x => x.GraceDays)
                                .NotNull()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.GraceDays)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.GraceDays)} {rule.Error}");
                        RuleFor(x => x.ReminderWorkOrderDays)
                                .GreaterThanOrEqualTo(0)
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.ReminderWorkOrderDays)} {rule.Error}");
                        RuleFor(x => x.ReminderMaterialReqDays)
                                .GreaterThanOrEqualTo(0)
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.ReminderMaterialReqDays)} {rule.Error}");
                        RuleFor(x => x.IsDownTimeRequired)
                                .NotNull()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.IsDownTimeRequired)} {rule.Error}");
                        RuleFor(x => x.DownTimeEstimateHrs)
                                .NotNull()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.DownTimeEstimateHrs)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(CreatePreventiveSchedulerCommand.DownTimeEstimateHrs)} {rule.Error}")
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
                    case "PastDateValidation":
                     RuleFor(x => x.EffectiveDate)
                                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                                .WithMessage($"{rule.Error}"); 
                    break;
                      case "AlreadyExists":
                        RuleForEach(x => x.Activity)
                         .MustAsync(async (command, activityDto, context, cancellation) =>
                         {
                             return !await _preventiveSchedulerQuery.AlreadyExistsAsync(
                                 activityDto.ActivityId, 
                                 command.MachineGroupId                    
                             );
                         }) 
                         .WithMessage($"{rule.Error}");  
                         
                            break; 
                         case "MachineGroupValidation":
                                RuleFor(x => x.MachineGroupId)
                                  .MustAsync(async (MachineGroupId, cancellation) => 
                                 await _preventiveSchedulerQuery.MachingroupValidation(MachineGroupId))
                                .WithMessage($"{rule.Error}"); 
                    break;
                    default:                        
                        break;
                }
            }
        }
         private bool BeAValidDate(DateOnly date)
        {
        
            return date >= DateOnly.FromDateTime(DateTime.Today);
        }
    }
}