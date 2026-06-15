using FinanceManagement.Application.AccountTypeMaster.Commands.DeleteAccountTypeMaster;
using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.AccountTypeMaster
{
    public class DeleteAccountTypeMasterCommandValidator : AbstractValidator<DeleteAccountTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAccountTypeMasterQueryRepository _queryRepository;

        public DeleteAccountTypeMasterCommandValidator(IAccountTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
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
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteAccountTypeMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Account Type Master {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage(rule.Error);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
