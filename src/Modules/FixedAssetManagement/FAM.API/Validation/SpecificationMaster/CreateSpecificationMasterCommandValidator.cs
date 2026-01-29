using FAM.Application.SpecificationMaster.Commands.CreateSpecificationMaster;
using FAM.Domain.Entities;
using FAM.API.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.API.Validation.SpecificationMaster
{
    public class CreateSpecificationMasterCommandValidator : AbstractValidator<CreateSpecificationMasterCommand>
    {
         private readonly List<ValidationRule> _validationRules;

        public CreateSpecificationMasterCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            // Get max lengths dynamically using MaxLengthProvider            
            var specificationNameMaxLength = maxLengthProvider.GetMaxLength<SpecificationMasters>("SpecificationName")??50;            

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
                        RuleFor(x => x.SpecificationName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSpecificationMasterCommand.SpecificationName)} {rule.Error}");
                        RuleFor(x => x.AssetGroupId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateSpecificationMasterCommand.AssetGroupId)} {rule.Error}");                                        
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.SpecificationName)
                            .MaximumLength(specificationNameMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateSpecificationMasterCommand.SpecificationName)} {rule.Error} {specificationNameMaxLength}");                        
                        break;          
                    case "NumericOnly":       
                        RuleFor(x => x.AssetGroupId)
                        .InclusiveBetween(1, int.MaxValue)
                        .WithMessage($"{nameof(CreateSpecificationMasterCommand.AssetGroupId)} {rule.Error}");                      
                        break;
                    default:                        
                        break;
                }
            }
        }
    }
}