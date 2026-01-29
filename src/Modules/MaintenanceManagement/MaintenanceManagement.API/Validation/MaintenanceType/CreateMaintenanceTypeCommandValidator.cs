using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.CreateMaintenanceType;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.MaintenanceType
{
    public class CreateMaintenanceTypeCommandValidator : AbstractValidator<CreateMaintenanceTypeCommand> 
    {
          private readonly List<ValidationRule> _validationRules;
         private readonly IMaintenanceTypeCommandRepository _imaintenanceTypeCommandRepository;
        public CreateMaintenanceTypeCommandValidator(IMaintenanceTypeCommandRepository imaintenanceTypeCommandRepository,MaxLengthProvider maxLengthProvider)
        {
            _imaintenanceTypeCommandRepository = imaintenanceTypeCommandRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            var MaintenanceTypeMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.MaintenanceType>("TypeName") ?? 100;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.TypeName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMaintenanceTypeCommand.TypeName)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.TypeName)
                            .MaximumLength(MaintenanceTypeMaxLength)
                            .WithMessage($"{nameof(CreateMaintenanceTypeCommand.TypeName)} {rule.Error} {MaintenanceTypeMaxLength}");
                            break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.TypeName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateMaintenanceTypeCommand.TypeName)} {rule.Error}");
                        break;
                     case "AlreadyExists":
                            RuleFor(x => x.TypeName)
                           .MustAsync(async (TypeName, cancellation) => !await _imaintenanceTypeCommandRepository.ExistsByCodeAsync(TypeName))
                           .WithName("MachineTypeName")
                           .WithMessage($"{rule.Error}");
                            break; 
    
                }
            }
        }

    }
}