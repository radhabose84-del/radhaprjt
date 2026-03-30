using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.UpdateMaintenanceCategory;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MaintenanceCategory
{
    public class UpdateMaintenanceCategoryCommandValidator : AbstractValidator<UpdateMaintenanceCategoryCommand> 
    {
        
         private readonly List<ValidationRule> _validationRules;
         private readonly IMaintenanceCategoryCommandRepository _iMaintenanceCategoryCommandRepository;
         private readonly IMaintenanceCategoryQueryRepository _iMaintenanceCategoryQueryRepository;
        public UpdateMaintenanceCategoryCommandValidator(IMaintenanceCategoryCommandRepository iMaintenanceCategoryCommandRepository,MaxLengthProvider maxLengthProvider,IMaintenanceCategoryQueryRepository iMaintenanceCategoryQueryRepository)
        {
            _iMaintenanceCategoryCommandRepository = iMaintenanceCategoryCommandRepository;
            _iMaintenanceCategoryQueryRepository=iMaintenanceCategoryQueryRepository;
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
                            .WithMessage($"{nameof(UpdateMaintenanceCategoryCommand.CategoryName)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.CategoryName)
                            .MaximumLength(CategoryNameMaxLength)
                            .WithMessage($"{nameof(UpdateMaintenanceCategoryCommand.CategoryName)} {rule.Error} {CategoryNameMaxLength}");
                        RuleFor(x => x.Description)
                            .MaximumLength(DescriptioneMaxLength)
                            .WithMessage($"{nameof(UpdateMaintenanceCategoryCommand.Description)} {rule.Error} {DescriptioneMaxLength}");
                            break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.CategoryName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateMaintenanceCategoryCommand.CategoryName)} {rule.Error}");
                        break;
                      case "AlreadyExists":
                        RuleFor(x => x.CategoryName)
                            .MustAsync(async (x, CategoryName, cancellation) =>
                                !await _iMaintenanceCategoryCommandRepository.IsNameDuplicateAsync(CategoryName, x.Id))
                            .WithName("CategoryName")
                            .WithMessage($"{rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CategoryName));
                        break;
                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) =>
                                (await _iMaintenanceCategoryQueryRepository.GetByIdAsync(id)) != null)
                            .WithName("Id")
                            .WithMessage($"{rule.Error}")
                            .When(x => x.Id > 0 && !string.IsNullOrWhiteSpace(x.CategoryName));
                            break;
    
                }
            }
        }

    }
}