using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.DeleteRoleItemGroupMapping;

namespace UserManagement.Presentation.Validation.RoleItemGroupMapping
{
    public class DeleteRoleItemGroupMappingCommandValidator
        : AbstractValidator<DeleteRoleItemGroupMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public DeleteRoleItemGroupMappingCommandValidator(
            IRoleItemGroupMappingQueryRepository queryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules is null || _validationRules.Count == 0)
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
                            .WithMessage($"{nameof(DeleteRoleItemGroupMappingCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await queryRepository.NotFoundAsync(id))
                            .WithMessage($"RoleItemGroupMapping {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
