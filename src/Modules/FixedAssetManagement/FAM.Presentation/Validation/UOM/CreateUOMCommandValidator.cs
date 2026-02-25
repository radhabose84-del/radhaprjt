using FAM.Application.UOM.Command.CreateUOM;
using FAM.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.Presentation.Validation.UOM
{
    public class CreateUOMCommandValidator : AbstractValidator<CreateUOMCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public CreateUOMCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var UOMNameMaxLength = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.UOM>("UOMName") ?? 50;
            
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
                            .WithMessage($"{nameof(CreateUOMCommand.Code)} {rule.Error}");
                        RuleFor(x => x.UOMName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUOMCommand.UOMName)} {rule.Error}");
                        RuleFor(x => x.SortOrder)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUOMCommand.SortOrder)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.UOMName)
                            .MaximumLength(UOMNameMaxLength)
                            .WithMessage($"{nameof(CreateUOMCommand.UOMName)} {rule.Error}");
                        break;
                    case "MinLength":
                        RuleFor(x => x.UOMTypeId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateUOMCommand.UOMTypeId)} {rule.Error} {0}");   
                        RuleFor(x => x.SortOrder)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateUOMCommand.SortOrder)} {rule.Error} {0}");
                        break; 
                }
            }
        }
        
    }
}