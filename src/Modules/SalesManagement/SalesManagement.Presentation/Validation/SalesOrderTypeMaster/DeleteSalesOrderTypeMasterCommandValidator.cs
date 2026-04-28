using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.DeleteSalesOrderTypeMaster;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOrderTypeMaster
{
    public class DeleteSalesOrderTypeMasterCommandValidator
        : AbstractValidator<DeleteSalesOrderTypeMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOrderTypeMasterQueryRepository _queryRepository;

        public DeleteSalesOrderTypeMasterCommandValidator(
            ISalesOrderTypeMasterQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteSalesOrderTypeMasterCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"SalesOrderTypeMaster {rule.Error}");
                        break;

                    // No "SoftDelete" dependent-blocking case yet — Finance.TransactionTypeMaster.TypeId
                    // does not exist; will be added in a future story when that FK is created.

                    default:
                        break;
                }
            }
        }
    }
}
