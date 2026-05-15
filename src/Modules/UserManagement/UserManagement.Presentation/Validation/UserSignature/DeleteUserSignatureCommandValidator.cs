using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.DeleteUserSignature;

namespace UserManagement.Presentation.Validation.UserSignature
{
    public class DeleteUserSignatureCommandValidator : AbstractValidator<DeleteUserSignatureCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IUserSignatureQueryRepository _userSignatureQueryRepository;

        public DeleteUserSignatureCommandValidator(IUserSignatureQueryRepository userSignatureQueryRepository)
        {
            _userSignatureQueryRepository = userSignatureQueryRepository;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(DeleteUserSignatureCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _userSignatureQueryRepository.NotFoundAsync(id))
                            .WithMessage($"UserSignature {rule.Error}")
                            .When(x => x.Id > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
