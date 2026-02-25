using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.CreateMachineMaster;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MachineMaster
{
    public class CreateMachineMasterCommandValidator : AbstractValidator<CreateMachineMasterCommand> 
    {
        private readonly List<ValidationRule> _validationRules;
         private readonly IMachineMasterCommandRepository _iMachineMasterCommandRepository;
        public CreateMachineMasterCommandValidator(IMachineMasterCommandRepository iMachineMasterCommandRepository,MaxLengthProvider maxLengthProvider)
        {
            _iMachineMasterCommandRepository=iMachineMasterCommandRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            var MachineCodeNameMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.MachineMaster>("MachineCode") ?? 20;
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
                        RuleFor(x => x.MachineCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMachineMasterCommand.MachineCode)} {rule.Error}");
                        RuleFor(x => x.MachineName)
                           .NotEmpty()
                           .WithMessage($"{nameof(CreateMachineMasterCommand.MachineName)} {rule.Error}")
                           .MustAsync(async (command, machineName, cancellation) =>
                            !await _iMachineMasterCommandRepository.IsNameDuplicateAsync(machineName, command.MachineGroupId, 0))
                           .WithMessage("A MachineName with the same name already exists in this Machinegroup.");
                        RuleFor(x => x.ShiftMasterId)
                          .NotEmpty()
                          .WithMessage($"{nameof(CreateMachineMasterCommand.ShiftMasterId)} {rule.Error} {0}");
                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMachineMasterCommand.UnitId)} {rule.Error} {0}");
                        RuleFor(x => x.MachineGroupId)
                             .NotEmpty()
                            .WithMessage($"{nameof(CreateMachineMasterCommand.MachineGroupId)} {rule.Error} {0}");
                        RuleFor(x => x.UomId)
                            .NotEmpty()
                           .WithMessage($"{nameof(CreateMachineMasterCommand.UomId)} {rule.Error} {0}");
                        RuleFor(x => x.CostCenterId)
                           .NotEmpty()
                           .WithMessage($"{nameof(CreateMachineMasterCommand.CostCenterId)} {rule.Error} {0}");
                        RuleFor(x => x.WorkCenterId)
                             .NotEmpty()
                            .WithMessage($"{nameof(CreateMachineMasterCommand.WorkCenterId)} {rule.Error} {0}");
                        RuleFor(x => x.AssetId)
                             .NotEmpty()
                            .WithMessage($"{nameof(CreateMachineMasterCommand.AssetId)} {rule.Error} {0}");
                        RuleFor(x => x.LineNo)
                            .NotEmpty()
                           .WithMessage($"{nameof(CreateMachineMasterCommand.LineNo)} {rule.Error} {0}");
                        RuleFor(x => x.InstallationDate)
                             .NotEmpty()
                            .WithMessage($"{nameof(CreateMachineMasterCommand.InstallationDate)} {rule.Error} {0}");
                        break;

                    case "MinLength":
                        RuleFor(x => x.ShiftMasterId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateMachineMasterCommand.ShiftMasterId)} {rule.Error} {0}");
                        RuleFor(x => x.UnitId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateMachineMasterCommand.UnitId)} {rule.Error} {0}");
                        RuleFor(x => x.MachineGroupId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateMachineMasterCommand.MachineGroupId)} {rule.Error} {0}");
                        RuleFor(x => x.UomId)
                           .GreaterThanOrEqualTo(1)
                           .WithMessage($"{nameof(CreateMachineMasterCommand.UomId)} {rule.Error} {0}");
                        RuleFor(x => x.CostCenterId)
                           .GreaterThanOrEqualTo(1)
                           .WithMessage($"{nameof(CreateMachineMasterCommand.CostCenterId)} {rule.Error} {0}");
                        RuleFor(x => x.WorkCenterId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateMachineMasterCommand.WorkCenterId)} {rule.Error} {0}");
                        RuleFor(x => x.AssetId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateMachineMasterCommand.AssetId)} {rule.Error} {0}");
                        RuleFor(x => x.LineNo)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateMachineMasterCommand.LineNo)} {rule.Error} {0}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.MachineCode)
                            .MaximumLength(MachineCodeNameMaxLength)
                            .WithMessage($"{nameof(CreateMachineMasterCommand.MachineCode)} {rule.Error} {MachineCodeNameMaxLength}");
                        RuleFor(x => x.MachineName)
                           .MaximumLength(MachineNameCodeMaxLength)
                           .WithMessage($"{nameof(CreateMachineMasterCommand.MachineName)} {rule.Error} {MachineNameCodeMaxLength}");
                        break;

                    case "AlphanumericOnly":
                        RuleFor(x => x.MachineCode)
                       .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                       .WithMessage($"{nameof(CreateMachineMasterCommand.MachineCode)} {rule.Error}");
                        break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.MachineName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateMachineMasterCommand.MachineName)} {rule.Error}");
                        break;
                    case "AlreadyExists":
                        RuleFor(x => x.MachineCode)
                       .MustAsync(async (MachineCode, cancellation) => !await _iMachineMasterCommandRepository.ExistsByCodeAsync(MachineCode))
                       .WithName("MachineCode")
                       .WithMessage($"{rule.Error}");
                        break; 
                  
    
                }
            }

        }
    }
}