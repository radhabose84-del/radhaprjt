using UserManagement.API.Validation.Common;
using UserManagement.Application.Divisions.Commands.CreateDivision;
using UserManagement.Domain.Entities;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.Divisions
{
    public class CreateDivisionCommandValidator : AbstractValidator<CreateDivisionCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateDivisionCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            var ShortNameMaxLength = maxLengthProvider.GetMaxLength<Division>("ShortName") ?? 50;
            var DivisionMaxLength = maxLengthProvider.GetMaxLength<Division>("Name") ?? 100;
            
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
                        RuleFor(x => x.Name)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDivisionCommand.Name)} {rule.Error}");
                        RuleFor(x => x.ShortName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDivisionCommand.ShortName)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.Name)
                            .MaximumLength(DivisionMaxLength)
                            .WithMessage($"{nameof(CreateDivisionCommand.Name)} {rule.Error}");
                        RuleFor(x => x.ShortName)
                            .MaximumLength(ShortNameMaxLength)
                            .WithMessage($"{nameof(CreateDivisionCommand.ShortName)} {rule.Error}");
                        break;
                        case "MinLength":
                        RuleFor(x => x.CompanyId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateDivisionCommand.CompanyId)} {rule.Error} {0}");   
                        break; 
                }
            }
        }
    }
}