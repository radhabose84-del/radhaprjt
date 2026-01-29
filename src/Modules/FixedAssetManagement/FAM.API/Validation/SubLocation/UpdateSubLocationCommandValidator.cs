using FAM.Application.Location.Command.UpdateSubLocation;
using FAM.API.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.API.Validation.SubLocation
{
    public class UpdateSubLocationCommandValidator : AbstractValidator<UpdateSubLocationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public UpdateSubLocationCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var SubLocationNameMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.SubLocation>("SubLocationName") ?? 50;
            var DescriptionMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.SubLocation>("Description") ?? 100;

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
                            .WithMessage($"{nameof(UpdateSubLocationCommand.Code)} {rule.Error}");
                        RuleFor(x => x.SubLocationName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateSubLocationCommand.SubLocationName)} {rule.Error}");
                        // RuleFor(x => x.Description)
                        // .NotEmpty()
                        // .WithMessage($"{nameof(UpdateSubLocationCommand.Description)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.SubLocationName)
                            .MaximumLength(SubLocationNameMaxLength)
                            .WithMessage($"{nameof(UpdateSubLocationCommand.SubLocationName)} {rule.Error}");
                        RuleFor(x => x.Description)
                            .MaximumLength(DescriptionMaxLength)
                            .WithMessage($"{nameof(UpdateSubLocationCommand.Description)} {rule.Error}");
                        break;
                    case "MinLength":
                        RuleFor(x => x.UnitId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateSubLocationCommand.UnitId)} {rule.Error} {0}");
                        RuleFor(x => x.DepartmentId)
                        .GreaterThanOrEqualTo(1)
                        .WithMessage($"{nameof(UpdateSubLocationCommand.DepartmentId)} {rule.Error} {0}");
                        RuleFor(x => x.LocationId)
                       .GreaterThanOrEqualTo(1)
                       .WithMessage($"{nameof(UpdateSubLocationCommand.LocationId)} {rule.Error} {0}");
                        break;
                }
            }

        }
    }
}