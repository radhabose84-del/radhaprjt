using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Application.CustomFields.Commands.DeleteCustomField;
using FluentValidation;
using Shared.Validation.Common;
using UserManagement.API.Validation.Common;

namespace UserManagement.API.Validation.CustomFields
{
    public class DeleteCustomFieldCommandValidator : AbstractValidator<DeleteCustomFieldCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICustomFieldQuery _customFieldQuery;
        public DeleteCustomFieldCommandValidator(ICustomFieldQuery customFieldQuery)
        {
             _validationRules = ValidationRuleLoader.LoadValidationRules();
             _customFieldQuery = customFieldQuery;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
             foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteCustomFieldCommand.Id)} {rule.Error}");
                        break;

                            case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _customFieldQuery.NotFoundAsync(Id))             
                           .WithName("Custom Field Id")
                            .WithMessage($"{rule.Error}");
                            break; 

                    default:
                        
                        break;
                        
                }
            }
        }
    }
}