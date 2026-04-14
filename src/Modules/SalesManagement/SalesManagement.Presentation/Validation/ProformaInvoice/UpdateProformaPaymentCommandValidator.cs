using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaPayment;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ProformaInvoice
{
    public class UpdateProformaPaymentCommandValidator : AbstractValidator<UpdateProformaPaymentCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProformaInvoiceQueryRepository _queryRepository;

        public UpdateProformaPaymentCommandValidator(IProformaInvoiceQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(UpdateProformaPaymentCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"ProformaInvoice {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.PaymentReceivedAmount)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateProformaPaymentCommand.PaymentReceivedAmount)} {rule.Error}");

                        // PaymentReceivedAmount must not exceed ProformaAmount
                        RuleFor(x => x)
                            .MustAsync(async (command, ct) =>
                            {
                                var proformaAmount = await _queryRepository.GetProformaAmountAsync(command.Id);
                                return command.PaymentReceivedAmount <= proformaAmount;
                            })
                            .WithMessage("Payment Received Amount cannot exceed the Proforma Amount.")
                            .When(x => x.Id > 0 && x.PaymentReceivedAmount > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
