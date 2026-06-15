using UserManagement.Presentation.Validation.Common;
using UserManagement.Application.IconMaster.Commands.UpdateIconMaster;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.IconMaster
{
    public class UpdateIconMasterCommandValidator : AbstractValidator<UpdateIconMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public UpdateIconMasterCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var iconNameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.IconMaster>("IconName") ?? 100;
            var iconLibraryMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.IconMaster>("IconLibrary") ?? 20;

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
                        RuleFor(x => x.IconName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateIconMasterCommand.IconName)} {rule.Error}");
                        RuleFor(x => x.IconLibrary)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateIconMasterCommand.IconLibrary)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.IconName)
                            .MaximumLength(iconNameMaxLength)
                            .WithMessage($"{nameof(UpdateIconMasterCommand.IconName)} {rule.Error} {iconNameMaxLength}");
                        RuleFor(x => x.IconLibrary)
                            .MaximumLength(iconLibraryMaxLength)
                            .WithMessage($"{nameof(UpdateIconMasterCommand.IconLibrary)} {rule.Error} {iconLibraryMaxLength}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.Size)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateIconMasterCommand.Size)} {rule.Error}");
                        break;

                    default:
                        Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                        break;
                }
            }
        }
    }
}
