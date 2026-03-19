using FluentValidation;
using UserManagement.Domain.Entities;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Application.State.Commands.UpdateState;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.State
{
    public class UpdateStateCommandValidator : AbstractValidator<UpdateStateCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public UpdateStateCommandValidator(MaxLengthProvider maxLengthProvider)
        { 
            // Get max lengths dynamically using MaxLengthProvider
            var stateCodeMaxLength = maxLengthProvider.GetMaxLength<States>("StateCode") ?? 5;
            var stateNameMaxLength = maxLengthProvider.GetMaxLength<States>("StateName") ?? 50;

            // Load validation rules from JSON or another source
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules is null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            // Loop through the rules and apply them
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateStateCommand.Id)} {rule.Error}");
                        RuleFor(x => x.StateName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateStateCommand.StateName)} {rule.Error}");
                        RuleFor(x => x.StateCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateStateCommand.StateCode)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.StateName)
                            .MaximumLength(stateNameMaxLength)
                            .WithMessage($"{nameof(UpdateStateCommand.StateName)} {rule.Error} {stateNameMaxLength}");
                        RuleFor(x => x.StateCode)
                            .MaximumLength(stateCodeMaxLength)
                            .WithMessage($"{nameof(UpdateStateCommand.StateCode)} {rule.Error} {stateCodeMaxLength}");
                        break;                                 
                    default:                        
                        break;
                }
            }
        }
    }
}