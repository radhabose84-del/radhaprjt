using FluentValidation;
using UserManagement.Domain.Entities;
using UserManagement.Application.Modules.Commands.CreateModule;
using UserManagement.Presentation.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Module
{
    public class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
    {
         private readonly List<ValidationRule> _validationRules;
         public CreateModuleCommandValidator(MaxLengthProvider maxLengthProvider)
         {
            var MaxLen = maxLengthProvider.GetMaxLength<Modules>("ModuleName") ?? 50;
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
                        RuleFor(x => x.ModuleName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateModuleCommand.ModuleName)} {rule.Error}");
                        break; 
                        case "MaxLength":
                        RuleFor(x => x.ModuleName)
                            .MaximumLength(MaxLen) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateModuleCommand.ModuleName)} {rule.Error} {MaxLen}");   
                        break;
                                  
                        default:
                        Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                        break;
                }
            }
         }
        
    }
}