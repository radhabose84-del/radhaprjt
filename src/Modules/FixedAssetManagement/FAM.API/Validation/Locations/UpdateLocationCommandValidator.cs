using FAM.API.Validation.Common;
using FAM.Domain.Entities;
using FluentValidation;
using FAM.Application.Location.Command.UpdateLocation;
using Shared.Validation.Common;

namespace FAM.API.Validation.Locations
{
    public class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public UpdateLocationCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var LocationNameMaxLength = maxLengthProvider.GetMaxLength<Location>("LocationName") ?? 50;
            var DescriptionMaxLength = maxLengthProvider.GetMaxLength<Location>("Description") ?? 100;

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
                        // Apply NotEmpty validation
                        RuleFor(x => x.Code)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateLocationCommand.Code)} {rule.Error}");
                        RuleFor(x => x.LocationName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateLocationCommand.LocationName)} {rule.Error}");
                            // RuleFor(x => x.Description)
                            // .NotEmpty()
                            // .WithMessage($"{nameof(UpdateLocationCommand.Description)} {rule.Error}");
                            RuleFor(x => x.SortOrder)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateLocationCommand.SortOrder)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.LocationName)
                            .MaximumLength(LocationNameMaxLength)
                            .WithMessage($"{nameof(UpdateLocationCommand.LocationName)} {rule.Error}");
                        RuleFor(x => x.Description)
                            .MaximumLength(DescriptionMaxLength)
                            .WithMessage($"{nameof(UpdateLocationCommand.Description)} {rule.Error}");
                        break;
                        case "MinLength":
                        RuleFor(x => x.UnitId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateLocationCommand.UnitId)} {rule.Error} {0}");   
                            RuleFor(x => x.DepartmentId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateLocationCommand.DepartmentId)} {rule.Error} {0}");   
                        break; 
                }
            }
        }
    }
}