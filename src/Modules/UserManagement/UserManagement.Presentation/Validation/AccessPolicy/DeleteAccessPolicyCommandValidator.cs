using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Application.AccessPolicy.Commands.DeleteAccessPolicy;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;

namespace UserManagement.Presentation.Validation.AccessPolicy
{
    public class DeleteAccessPolicyCommandValidator : AbstractValidator<DeleteAccessPolicyCommand>
    {
        private readonly List<ValidationRule>        _validationRules;
        private readonly IAccessPolicyQueryRepository _queryRepository;

        public DeleteAccessPolicyCommandValidator(IAccessPolicyQueryRepository queryRepository)
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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteAccessPolicyCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"AccessPolicy {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
