using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.CreateMachineGroup;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MachineGroup
{
    public class CreateMachineGroupCommandValidator : AbstractValidator<CreateMachineGroupCommand>
    {
      private readonly List<ValidationRule> _validationRules;            
      private readonly IMachineGroupQueryRepository _machineGroupQueryRepository;   
        
        public CreateMachineGroupCommandValidator(IMachineGroupQueryRepository machineGroupQueryRepository ,MaxLengthProvider maxLengthProvider)
        {
           var maxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.MachineGroup>("GroupName") ?? 50;

             _machineGroupQueryRepository = machineGroupQueryRepository;
        

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
                            .WithMessage($"{nameof(CreateMachineGroupCommand.GroupName)} {rule.Error}");
                         RuleFor(x => x.DepartmentId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMachineGroupCommand.DepartmentId)} {rule.Error}");
                        break;
                  case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.GroupName)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(CreateMachineGroupCommand.GroupName)} {rule.Error}");
                            break;

                    case "MinLength":
                        RuleFor(x => x.DepartmentId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateMachineGroupCommand.DepartmentId)} {rule.Error} {0}");   
                        break;
                            case "AlphanumericOnly": 
                        RuleFor(x => x.GroupName)
                            .Matches(new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9 ]+$")) // Allow spaces
                             .WithMessage($"{nameof(CreateMachineGroupCommand.GroupName)} {rule.Error}");
                        break;
                         case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.GroupName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateMachineGroupCommand.GroupName)} {rule.Error}");
                        break;  
                 
                  case "AlreadyExists":
                        RuleFor(x => x.GroupName)
                            .MustAsync(async (groupName, cancellation) =>
                                !await _machineGroupQueryRepository.GetByMachineGroupnameAsync(groupName))
                            .WithMessage("Group name already exists.");
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