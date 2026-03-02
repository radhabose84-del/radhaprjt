using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrder;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOrder
{
    public class DeleteSalesOrderCommandValidator : AbstractValidator<DeleteSalesOrderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOrderQueryRepository _queryRepository;

        public DeleteSalesOrderCommandValidator(ISalesOrderQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteSalesOrderCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"SalesOrder {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
