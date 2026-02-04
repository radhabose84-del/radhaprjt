using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using UserManagement.Application.State.Commands.CreateState;
using UserManagement.Domain.Entities;
using UserManagement.API.Validation.Common;
using UserManagement.Application.State.Commands.UpdateState;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.State
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
                        // Apply NotEmpty validation
                        RuleFor(x => x.StateName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateStateCommand.StateName)} {rule.Error}");
                        RuleFor(x => x.StateCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateStateCommand.StateCode)} {rule.Error}");
                        break;

                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.StateName)
                            .MaximumLength(stateNameMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateStateCommand.StateName)} {rule.Error} {stateNameMaxLength}");
                        RuleFor(x => x.StateCode)
                            .MaximumLength(stateCodeMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateStateCommand.StateCode)} {rule.Error} {stateCodeMaxLength}");
                        break;                                 
                    default:                        
                        break;
                }
            }
        }
    }
}