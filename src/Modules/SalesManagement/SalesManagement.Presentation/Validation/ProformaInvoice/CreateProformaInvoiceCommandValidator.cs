using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.CreateProformaInvoice;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ProformaInvoice
{
    public class CreateProformaInvoiceCommandValidator : AbstractValidator<CreateProformaInvoiceCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProformaInvoiceQueryRepository _queryRepository;

        public CreateProformaInvoiceCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IProformaInvoiceQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.ProformaInvoice>("Remarks") ?? 500;

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
                        RuleFor(x => x.SalesOrderId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateProformaInvoiceCommand.SalesOrderId)} {rule.Error}");

                        RuleFor(x => x.PartyId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateProformaInvoiceCommand.PartyId)} {rule.Error}");

                        RuleFor(x => x.StatusId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateProformaInvoiceCommand.StatusId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateProformaInvoiceCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        // Sales Order must exist and be Approved
                        RuleFor(x => x.SalesOrderId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesOrderExistsAndApprovedAsync(id))
                            .WithMessage("Sales Order does not exist or is not in Approved status.")
                            .When(x => x.SalesOrderId > 0);

                        // Sales Order must have Advance payment type
                        RuleFor(x => x.SalesOrderId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesOrderHasAdvancePaymentTypeAsync(id))
                            .WithMessage("Sales Order does not have Advance payment type. Proforma Invoice can only be generated for Advance payment Sales Orders.")
                            .When(x => x.SalesOrderId > 0);

                        // StatusId must exist in MiscMaster under ProformaInvStatus type
                        RuleFor(x => x.StatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.StatusExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateProformaInvoiceCommand.StatusId)} {rule.Error}")
                            .When(x => x.StatusId.HasValue && x.StatusId > 0);
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.ProformaAmount)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateProformaInvoiceCommand.ProformaAmount)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        // ProformaAmount must not exceed remaining SO balance
                        RuleFor(x => x.ProformaAmount)
                            .MustAsync(async (command, amount, ct) =>
                            {
                                var balance = await _queryRepository.GetSalesOrderBalanceAsync(command.SalesOrderId);
                                return amount <= balance;
                            })
                            .WithMessage("Proforma Amount exceeds the remaining Sales Order balance.")
                            .When(x => x.SalesOrderId > 0 && x.ProformaAmount > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
