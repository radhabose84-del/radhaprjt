using FluentValidation;
using UserManagement.Domain.Entities;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Application.City.Commands.UpdateCity;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.City
{
    public class UpdateCityCommandValidator : AbstractValidator<UpdateCityCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public UpdateCityCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var cityCodeMaxLength = maxLengthProvider.GetMaxLength<Cities>("CityCode") ?? 5;
            var cityNameMaxLength = maxLengthProvider.GetMaxLength<Cities>("CityName") ?? 50;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules is null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateCityCommand.Id)} {rule.Error}");
                        RuleFor(x => x.CityName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateCityCommand.CityName)} {rule.Error}");
                        RuleFor(x => x.CityCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateCityCommand.CityCode)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CityName)
                            .MaximumLength(cityNameMaxLength)
                            .WithMessage($"{nameof(UpdateCityCommand.CityName)} {rule.Error} {cityNameMaxLength}");
                        RuleFor(x => x.CityCode)
                            .MaximumLength(cityCodeMaxLength)
                            .WithMessage($"{nameof(UpdateCityCommand.CityCode)} {rule.Error} {cityCodeMaxLength}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween((byte)0, (byte)1)
                            .WithMessage($"{nameof(UpdateCityCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}