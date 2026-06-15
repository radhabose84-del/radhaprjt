using FluentValidation;
using FinanceManagement.Application.AccountGroup.Commands.DeleteAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.AccountGroup
{
    public class DeleteAccountGroupCommandValidator : AbstractValidator<DeleteAccountGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IAccountGroupQueryRepository _queryRepository;

        public DeleteAccountGroupCommandValidator(IAccountGroupQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteAccountGroupCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Account Group {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // A parent group cannot be deleted while it still has sub-groups.
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => !await _queryRepository.HasChildrenAsync(id))
                .WithMessage("Cannot delete a group that has sub-groups. Remove or move the children first.")
                .When(x => x.Id > 0);
        }
    }
}
