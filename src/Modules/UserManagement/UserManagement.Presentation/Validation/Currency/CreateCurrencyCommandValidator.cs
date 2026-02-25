using UserManagement.Presentation.Validation.Common;
using UserManagement.Application.Currency.Commands.CreateCurrency;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Currency
{
    public class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
    {
         private readonly List<ValidationRule> _validationRules;
        public CreateCurrencyCommandValidator(MaxLengthProvider maxLengthProvider)
        {
                 // Get max lengths dynamically using MaxLengthProvider
            var currencyNameMinLength = 3;
            var currencyCodeMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Currency>("Code") ?? 6;
            var currencyNameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.Currency>("Name") ?? 50;
             // Load validation rules from JSON or another source
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
                            .WithMessage($"{nameof(CreateCurrencyCommand.Code)} {rule.Error}");
                        RuleFor(x => x.Name)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCurrencyCommand.Name)} {rule.Error}");
                            break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.Name)
                            .MaximumLength(currencyNameMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateCurrencyCommand.Name)} {rule.Error} {currencyNameMaxLength}");
                        RuleFor(x => x.Code)
                            .MaximumLength(currencyCodeMaxLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateCurrencyCommand.Code)} {rule.Error} {currencyCodeMaxLength}");
                            break;  
                    case "AlphabeticOnly":
                        // Apply AlphabeticOnly validation
                        RuleFor(x => x.Code)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                            .WithMessage($"{nameof(CreateCurrencyCommand.Code)} {rule.Error}");   
                          
                        RuleFor(x => x.Name)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                            .WithMessage($"{nameof(CreateCurrencyCommand.Name)} {rule.Error}");
                            break;
                    case "MinLength":
                        // Apply MinLength validation
                        RuleFor(x => x.Code)
                            .MinimumLength(currencyNameMinLength) // Dynamic value from MaxLengthProvider
                            .WithMessage($"{nameof(CreateCurrencyCommand.Code)} {rule.Error} {currencyNameMinLength}");
                            break;
                    default:
                        // Handle unknown rule (log or throw)
                        Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                        break;
                }
            }
        }
    }
}