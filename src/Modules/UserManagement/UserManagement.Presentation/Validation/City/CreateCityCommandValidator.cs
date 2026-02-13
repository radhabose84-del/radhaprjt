using System;
using System.Collections.Generic;
using System.Linq;
using UserManagement.Application.City.Commands.CreateCity;
using UserManagement.Domain.Entities;
using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.Presentation.Validation.City
{
    public class CreateCityCommandValidator : AbstractValidator<CreateCityCommand>
    {
        private readonly List<ValidationRule> _validationRules;        
        public CreateCityCommandValidator(IMaxLengthProvider maxLengthProvider, IValidationRuleProvider ruleProvider)
        {
            if (maxLengthProvider is null) throw new ArgumentNullException(nameof(maxLengthProvider));
            if (ruleProvider is null) throw new ArgumentNullException(nameof(ruleProvider));

            var cityCodeMaxLength = maxLengthProvider.GetMaxLength<Cities>("CityCode") ?? 5;
            var cityNameMaxLength = maxLengthProvider.GetMaxLength<Cities>("CityName") ?? 50;

            _validationRules = ruleProvider.GetRules()?.ToList() ?? new List<ValidationRule>();

            if (_validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.CityName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCityCommand.CityName)} {rule.Error}");

                        RuleFor(x => x.CityCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCityCommand.CityCode)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CityName)
                            .MaximumLength(cityNameMaxLength)
                            .WithMessage($"{nameof(CreateCityCommand.CityName)} {rule.Error} {cityNameMaxLength}");

                        // ✅ FIX: use cityCodeMaxLength (not cityNameMaxLength)
                        RuleFor(x => x.CityCode)
                            .MaximumLength(cityCodeMaxLength)
                            .WithMessage($"{nameof(CreateCityCommand.CityCode)} {rule.Error} {cityCodeMaxLength}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
