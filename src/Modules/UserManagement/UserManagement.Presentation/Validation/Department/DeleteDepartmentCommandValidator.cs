using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Departments.Commands.DeleteDepartment;
using FluentValidation;
using Shared.Validation.Common;

namespace UserManagement.Presentation.Validation.Department
{
    public class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDepartmentQueryRepository _departmentQueryRepository;

        public DeleteDepartmentCommandValidator(IDepartmentQueryRepository departmentQueryRepository)
        {
            _departmentQueryRepository = departmentQueryRepository;
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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteDepartmentCommand.Id)} {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _departmentQueryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
