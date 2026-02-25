using InventoryManagement.Application.UOM.Command.CreateUOM;
using FluentValidation;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.UOM
{
    public class CreateUOMCommandValidator  : AbstractValidator<CreateUOMCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public CreateUOMCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var UOMNameMaxLength = maxLengthProvider.GetMaxLength<InventoryManagement.Domain.Entities.UOM>("UOMName") ?? 50;
            
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
            //  Common character validation – only alphanumeric + spaces (no special chars)
            const string alphaNumWithSpacePattern = @"^[A-Za-z0-9 ]+$";

            RuleFor(x => (x.Code ?? string.Empty).Trim())
                .Matches(alphaNumWithSpacePattern)
                .When(x => !string.IsNullOrWhiteSpace(x.Code))
                .WithMessage("Special characters are not allowed only alphanumeric values are allowed");

            RuleFor(x => (x.UOMName ?? string.Empty).Trim())
                .Matches(alphaNumWithSpacePattern)
                .When(x => !string.IsNullOrWhiteSpace(x.UOMName))
                .WithMessage("Special characters are not allowed only alphanumeric values are allowed");
        }
        
        
    }
}