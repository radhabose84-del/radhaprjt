using FluentValidation;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.GenerateEwb;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.EInvoiceHeader
{
    public class GenerateEwbCommandValidator : AbstractValidator<GenerateEwbCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IEInvoiceHeaderQueryRepository _queryRepository;

        public GenerateEwbCommandValidator(IEInvoiceHeaderQueryRepository queryRepository)
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
                        RuleFor(x => x.EInvoiceHeaderId)
                            .NotEmpty()
                            .WithMessage($"{nameof(GenerateEwbCommand.EInvoiceHeaderId)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.EInvoiceHeaderId)
                            .GreaterThan(0).WithMessage("Valid EInvoiceHeaderId is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"EInvoice Header {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.Distance)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(GenerateEwbCommand.Distance)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
