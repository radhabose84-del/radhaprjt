using FluentValidation;
using UserManagement.Domain.Entities;
using UserManagement.API.Validation.Common;
using UserManagement.Application.City.Commands.UpdateCity;
using UserManagement.Application.City.Commands.CreateCity;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.Country
{
    public class UpdateCityCommandValidator : AbstractValidator<UpdateCityCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public UpdateCityCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            // Get max lengths dynamically using MaxLengthProvider
            var cityCodeMaxLength = maxLengthProvider.GetMaxLength<Cities>("CityCode") ?? 5;
            var cityNameMaxLength = maxLengthProvider.GetMaxLength<Cities>("CityName") ?? 50;

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
                        RuleFor(x => x.CityName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCityCommand.CityName)} {rule.Error}");
                        RuleFor(x => x.CityCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCityCommand.CityCode)} {rule.Error}");
                        break;

                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.CityName)
                            .MaximumLength(cityNameMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateCityCommand.CityName)} {rule.Error} {cityNameMaxLength}");
                        RuleFor(x => x.CityCode)
                            .MaximumLength(cityCodeMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateCityCommand.CityCode)} {rule.Error} {cityCodeMaxLength}");
                        break;                                 
                    default:                                               
                        break;
                }
            }
        }
    }
}