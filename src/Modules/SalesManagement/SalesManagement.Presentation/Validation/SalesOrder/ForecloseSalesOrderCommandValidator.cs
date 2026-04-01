using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Commands.ForecloseSalesOrder;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesOrder
{
    public class ForecloseSalesOrderCommandValidator : AbstractValidator<ForecloseSalesOrderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesOrderQueryRepository _queryRepository;

        public ForecloseSalesOrderCommandValidator(ISalesOrderQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(ForecloseSalesOrderCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"SalesOrder {rule.Error}");
                        break;

                    case "DispatchAdviceRequired":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => await _queryRepository.HasDispatchAdviceAsync(id))
                            .WithMessage(rule.Error)
                            .When(x => x.Id > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
