#nullable disable
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.UpdateMachineGroup;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MachineGroup
{
    public class UpdateMachineGroupCommandValidator : AbstractValidator<UpdateMachineGroupCommand>
    {
        
      private readonly List<ValidationRule> _validationRules;            
      private readonly IMachineGroupQueryRepository _machineGroupQueryRepository;        

        public UpdateMachineGroupCommandValidator(IMachineGroupQueryRepository machineGroupQueryRepository,MaxLengthProvider maxLengthProvider)
        {            
         _machineGroupQueryRepository = machineGroupQueryRepository;
          var maxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.MachineGroup>("GroupName") ?? 50;  

            _validationRules = ValidationRuleLoader.LoadValidationRules(); 

             if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

             foreach (var rule in _validationRules)
            {
                 switch (rule.Rule)
                { 
                  case "NotEmpty":
                        // Apply NotEmpty validation
                        RuleFor(x => x.GroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineGroupCommand.GroupName)} {rule.Error}");
                        RuleFor(x => x.DepartmentId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineGroupCommand.DepartmentId)} {rule.Error}");
                        break;
                  case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.GroupName)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(UpdateMachineGroupCommand.GroupName)} {rule.Error}");
                            break;
                   case "MinLength":
                        RuleFor(x => x.DepartmentId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateMachineGroupCommand.DepartmentId)} {rule.Error} {0}");   
                        break;
                  case "AlphanumericOnly": 
                        RuleFor(x => x.GroupName)
                            .Matches(new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9 ]+$")) // Allow spaces
                             .WithMessage($"{nameof(UpdateMachineGroupCommand.GroupName)} {rule.Error}");
                        break;
                         case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.GroupName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateMachineGroupCommand.GroupName)} {rule.Error}");
                        break;  
                  case "AlreadyExists":
                    RuleFor(x => x)
                        .MustAsync(async (updateCommand, cancellation) =>
                            !await _machineGroupQueryRepository.GetByMachineGroupCodeAsync(updateCommand.GroupName, updateCommand.Id)
                        )
                        .WithMessage($"{rule.Error}");
                    break;
                  default:
                        // Handle unknown rule (log or throw)
                        Log.Information("Warning: Unknown rule '{Rule}' encountered.", rule.Rule);
                        break;            

                }

           
             
          }

            

        

    }
  }

}