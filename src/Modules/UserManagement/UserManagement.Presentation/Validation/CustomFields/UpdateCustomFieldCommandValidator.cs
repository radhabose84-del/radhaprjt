using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BSOFT.Infrastructure.Migrations;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.CustomFields.Commands.UpdateCustomField;
using UserManagement.Domain.Entities;
using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.Presentation.Validation.CustomFields
{
    public class UpdateCustomFieldCommandValidator : AbstractValidator<UpdateCustomFieldCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICustomFieldQuery _customFieldQuery;
        private readonly IUnitQueryRepository _unitQueryRepository;
        private readonly IMenuQuery _menuQuery;
        public UpdateCustomFieldCommandValidator(MaxLengthProvider maxLengthProvider, ICustomFieldQuery customFieldQuery, IUnitQueryRepository unitQueryRepository, IMenuQuery menuQuery)
        {
              var LabelNameMaxLength = maxLengthProvider.GetMaxLength<CustomField>("LabelName") ?? 50;

             _validationRules = ValidationRuleLoader.LoadValidationRules();
             _customFieldQuery = customFieldQuery;
            _unitQueryRepository = unitQueryRepository;
            _menuQuery = menuQuery;
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
                            .WithMessage($"{rule.Error}");
                        RuleFor(x => x.LabelName)
                            .NotEmpty()
                            .WithMessage($"{rule.Error}");
                        RuleFor(x => x.DataTypeId)
                            .NotEmpty()
                            .WithMessage($"{rule.Error}");
                        RuleFor(x => x.LabelTypeId)
                            .NotEmpty()
                            .WithMessage($"{rule.Error}");
                        RuleFor(x => x.Menu)
                            .NotNull()
                            .WithMessage($"{rule.Error}")
                            .Must(x => x.Count > 0)
                            .WithMessage($"{rule.Error}");
                        RuleFor(x => x.Unit)
                            .NotNull()
                            .WithMessage($"{rule.Error}")
                            .Must(x => x.Count > 0)
                            .WithMessage($"{rule.Error}");
                        RuleForEach(x => x.OptionalValues)
                             .ChildRules(optionRule =>
                             {
                                 optionRule.RuleFor(m => m.OptionFieldValue)
                                 .NotEmpty()
                                 .WithMessage($"{rule.Error}");  
                             })
                             .When(x => x.OptionalValues != null);
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.LabelName)
                            .MaximumLength(LabelNameMaxLength)
                            .WithMessage($"{rule.Error}");
                        break;
                        case "MinLength":
                        RuleFor(x => x.DataTypeId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{rule.Error} {0}");
                        RuleFor(x => x.LabelTypeId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{rule.Error} {0}");   
                        break; 
                          case "AlreadyExists":
                           RuleFor(x =>  new { x.LabelName, x.Id })
                           .MustAsync(async (customfield, cancellation) => 
                        !await _customFieldQuery.AlreadyExistsAsync(customfield.LabelName, customfield.Id))             
                           .WithName("Label Name")
                            .WithMessage($"{rule.Error}");
                            break; 

                            case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _customFieldQuery.NotFoundAsync(Id))             
                           .WithName("Custom Field Id")
                            .WithMessage($"{rule.Error}");
                            break; 
                 case "FKColumnDelete":
                        RuleForEach(x => x.Unit)
                             .ChildRules(unitRule =>
                             {
                                 unitRule.RuleFor(m => m.UnitId)
                                 .MustAsync(async (unitid, cancellation) => 
                                     await _unitQueryRepository.FKColumnExistValidation(unitid))
                                     .WithMessage($"{rule.Error}");  
                             });
                        RuleForEach(x => x.Menu)
                             .ChildRules(menuRule =>
                             {
                                 menuRule.RuleFor(m => m.MenuId)
                                 .MustAsync(async (id, cancellation) => 
                                     await _menuQuery.FKColumnExistValidation(id))
                                     .WithMessage($"{rule.Error}");  
                             });
                             
                        break; 
                    default:
                        
                        break;
                        
                }
            }
        }
        
    }
}