using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Commands.DeleteSalesQuotation;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesQuotation
{
    public class DeleteSalesQuotationCommandValidator : AbstractValidator<DeleteSalesQuotationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesQuotationQueryRepository _queryRepository;

        public DeleteSalesQuotationCommandValidator(ISalesQuotationQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteSalesQuotationCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"SalesQuotation {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
