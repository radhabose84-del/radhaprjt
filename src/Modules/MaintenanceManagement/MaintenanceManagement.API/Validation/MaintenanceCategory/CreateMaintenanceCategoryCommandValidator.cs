using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.CreateMaintenanceCategory;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.MaintenanceCategory
{
    public class CreateMaintenanceCategoryCommandValidator : AbstractValidator<CreateMaintenanceCategoryCommand> 
    {
          private readonly List<ValidationRule> _validationRules;
         private readonly IMaintenanceCategoryCommandRepository _iMaintenanceCategoryCommandRepository;
           public CreateMaintenanceCategoryCommandValidator(IMaintenanceCategoryCommandRepository iMaintenanceCategoryCommandRepository,MaxLengthProvider maxLengthProvider)
        {
            _iMaintenanceCategoryCommandRepository = iMaintenanceCategoryCommandRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            var CategoryNameMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.MaintenanceCategory>("CategoryName") ?? 100;
            var DescriptioneMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.MaintenanceCategory>("Description") ?? 250;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.CategoryName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMaintenanceCategoryCommand.CategoryName)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.CategoryName)
                            .MaximumLength(CategoryNameMaxLength)
                            .WithMessage($"{nameof(CreateMaintenanceCategoryCommand.CategoryName)} {rule.Error} {CategoryNameMaxLength}");
                         RuleFor(x => x.Description)
                            .MaximumLength(DescriptioneMaxLength)
                            .WithMessage($"{nameof(CreateMaintenanceCategoryCommand.Description)} {rule.Error} {DescriptioneMaxLength}");
                            break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.CategoryName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateMaintenanceCategoryCommand.CategoryName)} {rule.Error}");
                        break;
                     case "AlreadyExists":
                            RuleFor(x => x.CategoryName)
                           .MustAsync(async (CategoryName, cancellation) => !await _iMaintenanceCategoryCommandRepository.ExistsByCodeAsync(CategoryName))
                           .WithName("CategoryName")
                           .WithMessage($"{rule.Error}");
                            break; 
    
                }
            }
        }
    }
}