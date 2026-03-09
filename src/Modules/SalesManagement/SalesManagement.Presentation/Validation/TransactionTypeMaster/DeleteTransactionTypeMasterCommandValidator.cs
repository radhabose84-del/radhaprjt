using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using SalesManagement.Application.TransactionTypeMaster.Commands.DeleteTransactionTypeMaster;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.TransactionTypeMaster
{
    public class DeleteTransactionTypeMasterCommandValidator : AbstractValidator<DeleteTransactionTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ITransactionTypeMasterQueryRepository _queryRepository;

        public DeleteTransactionTypeMasterCommandValidator(ITransactionTypeMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteTransactionTypeMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Transaction Type Master {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
