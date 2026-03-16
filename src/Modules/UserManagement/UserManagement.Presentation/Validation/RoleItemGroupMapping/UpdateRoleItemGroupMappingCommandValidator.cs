using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.UpdateRoleItemGroupMapping;

namespace UserManagement.Presentation.Validation.RoleItemGroupMapping
{
    public class UpdateRoleItemGroupMappingCommandValidator
        : AbstractValidator<UpdateRoleItemGroupMappingCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public UpdateRoleItemGroupMappingCommandValidator(
            IRoleItemGroupMappingCommandRepository commandRepository,
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
                        RuleFor(x => x.RoleId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRoleItemGroupMappingCommand.RoleId)} is required and must be greater than 0.");

                        RuleFor(x => x.ItemGroupId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRoleItemGroupMappingCommand.ItemGroupId)} is required and must be greater than 0.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await queryRepository.NotFoundAsync(id))
                            .WithMessage($"RoleItemGroupMapping {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await commandRepository.CompositeKeyExistsAsync(
                                    cmd.RoleId, cmd.ItemGroupId, cmd.Id))
                            .WithMessage("RoleItemGroupMapping with this RoleId and ItemGroupId combination already exists.")
                            .When(x => x.RoleId > 0 && x.ItemGroupId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateRoleItemGroupMappingCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
