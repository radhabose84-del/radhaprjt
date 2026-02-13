using FAM.Application.AssetMaster.AssetSpecification.Commands.UpdateAssetSpecification;
using FAM.Domain.Entities.AssetMaster;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.AssetMaster.AssetSpecification
{
    public class UpdateAssetSpecificationCommandValidator : AbstractValidator<UpdateAssetSpecificationCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public UpdateAssetSpecificationCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            RuleFor(x => x.AssetId)
                .GreaterThan(0)
                .WithMessage("AssetId must be greater than 0.");

            RuleFor(x => x.Specifications)
                .NotEmpty()
                .WithMessage("At least one specification must be provided.");

            var maxLength = maxLengthProvider.GetMaxLength<AssetSpecifications>("SpecificationValue") ?? 100;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules is null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            RuleForEach(x => x.Specifications).ChildRules(spec =>
            {
                foreach (var rule in _validationRules)
                {
                    switch (rule.Rule)
                    {
                        case "NotEmpty":
                            spec.RuleFor(s => s.SpecificationId)
                                .NotEmpty()
                                .WithMessage($"SpecificationId {rule.Error}");

                            spec.RuleFor(s => s.SpecificationValue)
                                .NotEmpty()
                                .WithMessage($"SpecificationValue {rule.Error}");
                            break;

                        case "MaxLength":
                            spec.RuleFor(s => s.SpecificationValue)
                                .MaximumLength(maxLength)
                                .WithMessage($"SpecificationValue {rule.Error} {maxLength}");
                            break;

                        case "NumericOnly":
                            spec.RuleFor(s => s.SpecificationId)
                                .InclusiveBetween(1, int.MaxValue)
                                .WithMessage($"SpecificationId {rule.Error}");
                            break;

                        default:
                            break;
                    }
                }
            });
        }
    }
}
