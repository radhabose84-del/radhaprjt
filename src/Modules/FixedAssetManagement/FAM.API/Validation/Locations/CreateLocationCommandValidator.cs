using FAM.API.Validation.Common;
using FAM.Domain.Entities;
using FluentValidation;
using FAM.Application.Location.Command.CreateLocation;
using Shared.Validation.Common;

namespace FAM.API.Validation.Locations
{
    public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateLocationCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var locationNameMaxLength = maxLengthProvider.GetMaxLength<Location>("LocationName") ?? 50;
            var descriptionMaxLength = maxLengthProvider.GetMaxLength<Location>("Description") ?? 100;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Code).NotEmpty()
                            .WithMessage($"{nameof(CreateLocationCommand.Code)} {rule.Error}");

                        RuleFor(x => x.LocationName).NotEmpty()
                            .WithMessage($"{nameof(CreateLocationCommand.LocationName)} {rule.Error}");

                        RuleFor(x => x.SortOrder).NotEmpty()
                            .WithMessage($"{nameof(CreateLocationCommand.SortOrder)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.LocationName)
                            .MaximumLength(locationNameMaxLength)
                            .WithMessage($"{nameof(CreateLocationCommand.LocationName)} {rule.Error} {locationNameMaxLength}");

                        RuleFor(x => x.Description)
                            .MaximumLength(descriptionMaxLength)
                            .WithMessage($"{nameof(CreateLocationCommand.Description)} {rule.Error} {descriptionMaxLength}");
                        break;

                    case "MinLength": // keeping your rule key as-is
                        RuleFor(x => x.UnitId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateLocationCommand.UnitId)} {rule.Error} 1");

                        RuleFor(x => x.DepartmentId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateLocationCommand.DepartmentId)} {rule.Error} 1");

                        RuleFor(x => x.SortOrder)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateLocationCommand.SortOrder)} {rule.Error} 1");
                        break;
                }
            }
        }
    }
}
