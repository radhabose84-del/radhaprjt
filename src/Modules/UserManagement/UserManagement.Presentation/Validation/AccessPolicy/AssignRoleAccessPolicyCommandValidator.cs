using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Application.AccessPolicy.Commands.AssignRoleAccessPolicy;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;

namespace UserManagement.Presentation.Validation.AccessPolicy
{
    public class AssignRoleAccessPolicyCommandValidator : AbstractValidator<AssignRoleAccessPolicyCommand>
    {
        private readonly List<ValidationRule>        _validationRules;
        private readonly IAccessPolicyQueryRepository _queryRepository;

        public AssignRoleAccessPolicyCommandValidator(IAccessPolicyQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.AccessPolicyId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(AssignRoleAccessPolicyCommand.AccessPolicyId)} {rule.Error}");
                        RuleFor(x => x.RoleId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(AssignRoleAccessPolicyCommand.RoleId)} {rule.Error}");
                        RuleFor(x => x.ValueId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(AssignRoleAccessPolicyCommand.ValueId)} {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.AccessPolicyId)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"{nameof(AssignRoleAccessPolicyCommand.AccessPolicyId)} {rule.Error}")
                            .When(x => x.AccessPolicyId > 0);

                        RuleFor(x => x.RoleId)
                            .MustAsync(async (id, ct) => await _queryRepository.UserRoleExistsAsync(id))
                            .WithMessage($"{nameof(AssignRoleAccessPolicyCommand.RoleId)} {rule.Error}")
                            .When(x => x.RoleId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                                !await _queryRepository.RoleValueAssignmentExistsAsync(
                                    cmd.AccessPolicyId, cmd.RoleId, cmd.ValueId))
                            .WithMessage("This Role-Value assignment for the given Access Policy already exists.")
                            .When(x => x.AccessPolicyId > 0 && x.RoleId > 0 && x.ValueId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
