#nullable disable
using UserManagement.Application.DepartmentGroup.Command.CreateDepartmentGroup;
using FluentValidation;
using UserManagement.Presentation.Validation.Common;
using Serilog;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using Shared.Validation.Common;


namespace UserManagement.Presentation.Validation.DepartmentGroup
{
    public class CreateDepartmentGroupCommandValidator : AbstractValidator<CreateDepartmentGroupCommand>
    {

           private readonly List<ValidationRule> _validationRules;
        private readonly IDepartmentGroupQueryRepository _departmentGroupQueryRepository;

        public CreateDepartmentGroupCommandValidator(MaxLengthProvider maxLengthProvider, IDepartmentGroupQueryRepository departmentGroupQueryRepository)
        {
            var GroupCodeMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.DepartmentGroup>("DepartmentGroupCode") ?? 15;
            var GroupNameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.DepartmentGroup>("DepartmentGroupName") ?? 50;
           
           _departmentGroupQueryRepository = departmentGroupQueryRepository;
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
                        RuleFor(x => x.DepartmentGroupName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDepartmentGroupCommand.DepartmentGroupName)} {rule.Error}");
                        break;
                    case "MaxLength":
                        // Apply MaxLength validation using dynamic max length values
                        RuleFor(x => x.DepartmentGroupName)
                            .MaximumLength(GroupNameMaxLength)
                            .WithMessage($"{nameof(CreateDepartmentGroupCommand.DepartmentGroupName)} {rule.Error} {GroupNameMaxLength}");
                        break;


                    default:
                        Log.Information($"Warning: Unknown rule '{rule.Rule}' encountered.");
                        break;
                }
            }

             // Uniqueness check
                RuleFor(x => x.DepartmentGroupName)
                    .MustAsync(async (name, cancellation) =>
                    {
                        var existing = await _departmentGroupQueryRepository.GetByDepartmentGroupNameAsync(name);
                        return existing == null;
                    })
                    .WithMessage("Department group name already exists.");
    

        }
    }
}