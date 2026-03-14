using FluentValidation;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.GenerateIrn;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.EInvoiceHeader
{
    public class GenerateIrnCommandValidator : AbstractValidator<GenerateIrnCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IEInvoiceHeaderQueryRepository _queryRepository;

        public GenerateIrnCommandValidator(IEInvoiceHeaderQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(GenerateIrnCommand.EInvoiceHeaderId)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.EInvoiceHeaderId)
                            .GreaterThan(0).WithMessage("Valid EInvoiceHeaderId is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"EInvoice Header {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
