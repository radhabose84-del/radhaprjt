using FAM.Application.AssetMaster.AssetSpecification.Commands.CreateAssetSpecification;
using FAM.Domain.Entities.AssetMaster;
using FAM.API.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.API.Validation.AssetMaster.AssetSpecification
{
    public class CreateAssetSpecificationCommandValidator : AbstractValidator<CreateAssetSpecificationCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateAssetSpecificationCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            RuleFor(x => x.AssetId)
                .GreaterThan(0).WithMessage("AssetId must be greater than 0");

            RuleFor(x => x.Specifications)
                .NotEmpty().WithMessage("At least one specification must be provided.");

            var assetSpecificationMaxLength = maxLengthProvider.GetMaxLength<AssetSpecifications>("SpecificationValue") ?? 100;

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
                            spec.RuleFor(x => x.SpecificationId)
                                .NotEmpty().WithMessage($"{nameof(SpecificationItem.SpecificationId)} {rule.Error}");
                            spec.RuleFor(x => x.SpecificationValue)
                                .NotEmpty().WithMessage($"{nameof(SpecificationItem.SpecificationValue)} {rule.Error}");
                            break;

                        case "MaxLength":
                            spec.RuleFor(x => x.SpecificationValue)
                                .MaximumLength(assetSpecificationMaxLength)
                                .WithMessage($"{nameof(SpecificationItem.SpecificationValue)} {rule.Error} {assetSpecificationMaxLength}");
                            break;

                        case "NumericOnly":
                            spec.RuleFor(x => x.SpecificationId)
                                .InclusiveBetween(1, int.MaxValue)
                                .WithMessage($"{nameof(SpecificationItem.SpecificationId)} {rule.Error}");
                            break;

                        default:
                            break;
                    }
                }
            });
        }
    }
}
