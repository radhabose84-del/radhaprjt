using UserManagement.API.Validation.Common;
using Core.Application.Divisions.Commands.UpdateDivision;
using Core.Domain.Entities;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.Divisions
{
    public class UpdateDivisionCommandValidator : AbstractValidator<UpdateDivisionCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public UpdateDivisionCommandValidator(MaxLengthProvider maxLengthProvider)
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
                            .WithMessage($"{nameof(UpdateDivisionCommand.Name)} {rule.Error}");
                        RuleFor(x => x.ShortName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDivisionCommand.ShortName)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.Name)
                            .MaximumLength(DivisionMaxLength)
                            .WithMessage($"{nameof(UpdateDivisionCommand.Name)} {rule.Error}");
                        RuleFor(x => x.ShortName)
                            .MaximumLength(ShortNameMaxLength)
                            .WithMessage($"{nameof(UpdateDivisionCommand.ShortName)} {rule.Error}");
                        break;
                        case "MinLength":
                        RuleFor(x => x.CompanyId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateDivisionCommand.CompanyId)} {rule.Error} {0}");   
                        break; 
                }
            }
        }
    }
}