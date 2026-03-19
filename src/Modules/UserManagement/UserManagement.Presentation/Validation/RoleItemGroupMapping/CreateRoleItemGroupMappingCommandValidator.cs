using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.CreateRoleItemGroupMapping;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.Presentation.Validation.RoleItemGroupMapping
{
    public class CreateRoleItemGroupMappingCommandValidator
        : AbstractValidator<CreateRoleItemGroupMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateRoleItemGroupMappingCommandValidator(
            IRoleItemGroupMappingCommandRepository commandRepository)
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
                        RuleFor(x => x.RoleId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRoleItemGroupMappingCommand.RoleId)} is required and must be greater than 0.");

                        RuleFor(x => x.ItemGroupId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRoleItemGroupMappingCommand.ItemGroupId)} is required and must be greater than 0.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await commandRepository.CompositeKeyExistsAsync(cmd.RoleId, cmd.ItemGroupId))
                            .WithMessage("RoleItemGroupMapping with this RoleId and ItemGroupId combination already exists.")
                            .When(x => x.RoleId > 0 && x.ItemGroupId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
