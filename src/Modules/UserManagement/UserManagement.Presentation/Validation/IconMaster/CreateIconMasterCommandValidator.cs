using UserManagement.Presentation.Validation.Common;
using UserManagement.Application.IconMaster.Commands.CreateIconMaster;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.IconMaster
{
    public class CreateIconMasterCommandValidator : AbstractValidator<CreateIconMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateIconMasterCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var keywordMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.IconMaster>("Keyword") ?? 50;
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
                        RuleFor(x => x.Keyword)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateIconMasterCommand.Keyword)} {rule.Error}");
                        RuleFor(x => x.IconName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateIconMasterCommand.IconName)} {rule.Error}");
                        RuleFor(x => x.IconLibrary)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateIconMasterCommand.IconLibrary)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Keyword)
                            .MaximumLength(keywordMaxLength)
                            .WithMessage($"{nameof(CreateIconMasterCommand.Keyword)} {rule.Error} {keywordMaxLength}");
                        RuleFor(x => x.IconName)
                            .MaximumLength(iconNameMaxLength)
                            .WithMessage($"{nameof(CreateIconMasterCommand.IconName)} {rule.Error} {iconNameMaxLength}");
                        RuleFor(x => x.IconLibrary)
                            .MaximumLength(iconLibraryMaxLength)
                            .WithMessage($"{nameof(CreateIconMasterCommand.IconLibrary)} {rule.Error} {iconLibraryMaxLength}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.Size)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateIconMasterCommand.Size)} {rule.Error}");
                        break;

                    default:
                        Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                        break;
                }
            }
        }
    }
}
