#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.CreateActivityCheckListMaster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.ActivityCheckListMaster
{
    public class CreateActivityCheckListMasterCommandValidator  : AbstractValidator<CreateActivityCheckListMasterCommand>
    {
         private readonly List<ValidationRule> _validationRules;            
      private readonly IActivityCheckListMasterQueryRepository _activityCheckListMasterQueryRepository;


       public CreateActivityCheckListMasterCommandValidator( IActivityCheckListMasterQueryRepository  activityCheckListMasterQueryRepository,MaxLengthProvider maxLengthProvider)
      {
          var maxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster>("ActivityCheckList") ?? 250;

          _validationRules = new List<ValidationRule>();
             _validationRules = ValidationRuleLoader.LoadValidationRules();
          _activityCheckListMasterQueryRepository = activityCheckListMasterQueryRepository;
           
           foreach (var rule in _validationRules)
            {
                 switch (rule.Rule)
                { 
                  case "NotEmpty":
                   RuleFor(x => x.ActivityCheckList)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateActivityCheckListMasterCommand.ActivityCheckList)} {rule.Error}");
                        break;
                  case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.ActivityCheckList)
                            .MaximumLength(maxLength)
                            .WithMessage($"{nameof(CreateActivityCheckListMasterCommand.ActivityCheckList)} {rule.Error}"); 
                            break;                   
                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (command, cancellation) =>
                                !await _activityCheckListMasterQueryRepository
                                    .GetByActivityCheckListAsync(command.ActivityCheckList, command.ActivityID))
                            .WithMessage("ActivityCheckList already exists.");
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