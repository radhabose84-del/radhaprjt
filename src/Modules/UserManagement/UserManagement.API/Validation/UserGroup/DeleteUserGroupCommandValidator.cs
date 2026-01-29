using Core.Application.Common.Interfaces.IUserGroup;
using Core.Application.UserGroup.Commands.DeleteUserGroup;
using FluentValidation;
using Shared.Validation.Common;
using UserManagement.API.Validation.Common;

namespace UserManagement.API.Validation.UserGroup
{
    public class DeleteUserGroupCommandValidator  : AbstractValidator<DeleteUserGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IUserGroupQueryRepository _userGroupQueryRepository;
        public DeleteUserGroupCommandValidator( IUserGroupQueryRepository userGroupQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _userGroupQueryRepository = userGroupQueryRepository;
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
                            .WithMessage($"{nameof(DeleteUserGroupCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                         RuleFor(x => x.Id)
                      .MustAsync(async (Id, cancellation) => !await _userGroupQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                    default:
                        
                        break;
                }
            }
        }
    }
}