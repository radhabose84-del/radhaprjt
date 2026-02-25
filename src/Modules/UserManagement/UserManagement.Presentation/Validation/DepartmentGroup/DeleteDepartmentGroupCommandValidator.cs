using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Command.DeleteDepartmentGroup;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.DepartmentGroup
{
    public class DeleteDepartmentGroupCommandValidator : AbstractValidator<DeleteDepartmentGroupCommand>
    {

        private readonly List<ValidationRule> _validationRules;
        private readonly IDepartmentGroupQueryRepository _departmentGroupQueryRepository;

          public DeleteDepartmentGroupCommandValidator(IDepartmentGroupQueryRepository departmentGroupQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _departmentGroupQueryRepository = departmentGroupQueryRepository ;
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
                            .WithMessage($"{nameof(DeleteDepartmentGroupCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                         RuleFor(x => x.Id)
                      .MustAsync(async (Id, cancellation) => !await _departmentGroupQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                    default:
                        
                    break;
                }
            }
        }
        
    }
}