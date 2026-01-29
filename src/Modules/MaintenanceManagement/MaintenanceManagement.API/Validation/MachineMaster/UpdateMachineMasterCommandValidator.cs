using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.UpdateMachineMaster;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.MachineMaster
{
    public class UpdateMachineMasterCommandValidator : AbstractValidator<UpdateMachineMasterCommand> 
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMachineMasterCommandRepository _iMachineMasterCommandRepository;
        private readonly IMachineMasterQueryRepository _iMachineMasterQueryRepository;
        
        public UpdateMachineMasterCommandValidator(IMachineMasterCommandRepository iMachineMasterCommandRepository,MaxLengthProvider maxLengthProvider,IMachineMasterQueryRepository iMachineMasterQueryRepository) 
        {
             _iMachineMasterCommandRepository=iMachineMasterCommandRepository;
             _iMachineMasterQueryRepository=iMachineMasterQueryRepository;
              _validationRules = ValidationRuleLoader.LoadValidationRules();
             var MachineNameCodeMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.MachineMaster>("MachineName") ?? 400;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                         RuleFor(x => x.MachineName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.MachineName)} {rule.Error}");  
                          RuleFor(x => x.ShiftMasterId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.ShiftMasterId)} {rule.Error} {0}");   
                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.UnitId)} {rule.Error} {0}");   
                        RuleFor(x => x.MachineGroupId)
                             .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.MachineGroupId)} {rule.Error} {0}");   
                         RuleFor(x => x.UomId)
                             .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.UomId)} {rule.Error} {0}"); 
                         RuleFor(x => x.CostCenterId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.CostCenterId)} {rule.Error} {0}"); 
                        RuleFor(x => x.WorkCenterId)
                             .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.WorkCenterId)} {rule.Error} {0}"); 
                        RuleFor(x => x.AssetId)
                             .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.AssetId)} {rule.Error} {0}"); 
                        RuleFor(x => x.LineNo)
                             .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.LineNo)} {rule.Error} {0}"); 
                        RuleFor(x => x.InstallationDate)
                             .NotEmpty()
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.InstallationDate)} {rule.Error} {0}"); 
                        break;
                     case "MinLength":
                        RuleFor(x => x.ShiftMasterId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.ShiftMasterId)} {rule.Error} {0}");   
                        RuleFor(x => x.UnitId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.UnitId)} {rule.Error} {0}");   
                        RuleFor(x => x.MachineGroupId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.MachineGroupId)} {rule.Error} {0}");   
                         RuleFor(x => x.UomId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.UomId)} {rule.Error} {0}"); 
                         RuleFor(x => x.CostCenterId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.CostCenterId)} {rule.Error} {0}"); 
                        RuleFor(x => x.WorkCenterId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.WorkCenterId)} {rule.Error} {0}"); 
                        RuleFor(x => x.AssetId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.AssetId)} {rule.Error} {0}"); 
                          RuleFor(x => x.LineNo)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.AssetId)} {rule.Error} {0}"); 
                        break;
                    case "MaxLength":
                         RuleFor(x => x.MachineName)
                            .MaximumLength(MachineNameCodeMaxLength)
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.MachineName)} {rule.Error} {MachineNameCodeMaxLength}");
                            break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.MachineName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateMachineMasterCommand.MachineName)} {rule.Error}");
                        break;
                    case "AlreadyExists":
                        RuleFor(x => x.MachineName)
                            .MustAsync(async (model, machineName, cancellation) =>
                                !await _iMachineMasterCommandRepository
                                    .IsNameDuplicateAsync(machineName, model.MachineGroupId, model.Id))
                            .WithName("MachineName")
                            .WithMessage("MachineName with the same name already exists in this Machinegroup.");
                            
                        RuleFor(x => x.MachineCode)
                            .MustAsync(async (model, machineCode, cancellation) =>
                                !await _iMachineMasterCommandRepository.IsCodeDuplicateAsync(
                                    machineCode,
                                    model.UnitId,   // ✅ include UnitId
                                    model.Id        // ✅ exclude current Id for update
                                ))
                                .WithName("MachineCode")
                                .WithMessage($"{rule.Error}");
                        break;
                      case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) => 
                                (await _iMachineMasterQueryRepository.GetByIdAsync(id)) != null) 
                            .WithName("Id")
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