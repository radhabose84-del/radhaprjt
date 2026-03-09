using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Commands.CreateInvoice;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.Invoice
{
    public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IInvoiceQueryRepository _queryRepository;

        public CreateInvoiceCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IInvoiceQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthVehicleNumber  = maxLengthProvider.GetMaxLength<Domain.Entities.InvoiceHeader>("VehicleNumber")  ?? 20;
            var maxLengthTransporterName = maxLengthProvider.GetMaxLength<Domain.Entities.InvoiceHeader>("TransporterName") ?? 100;
            var maxLengthLRNumber       = maxLengthProvider.GetMaxLength<Domain.Entities.InvoiceHeader>("LRNumber")       ?? 50;
            var maxLengthRemarks        = maxLengthProvider.GetMaxLength<Domain.Entities.InvoiceHeader>("Remarks")        ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.DispatchAdviceId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateInvoiceCommand.DispatchAdviceId)} {rule.Error}");

                        RuleFor(x => x.InvoiceType)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateInvoiceCommand.InvoiceType)} {rule.Error}");

                        RuleFor(x => x.PartyId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateInvoiceCommand.PartyId)} {rule.Error}");

                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateInvoiceCommand.UnitId)} {rule.Error}");

                        RuleFor(x => x.FinancialYearId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateInvoiceCommand.FinancialYearId)} {rule.Error}");

                        RuleFor(x => x.Details)
                            .NotNull()
                            .WithMessage($"Details {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"Details {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.VehicleNumber)
                            .MaximumLength(maxLengthVehicleNumber)
                            .WithMessage($"{nameof(CreateInvoiceCommand.VehicleNumber)} {rule.Error} {maxLengthVehicleNumber} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.VehicleNumber));

                        RuleFor(x => x.TransporterName)
                            .MaximumLength(maxLengthTransporterName)
                            .WithMessage($"{nameof(CreateInvoiceCommand.TransporterName)} {rule.Error} {maxLengthTransporterName} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.TransporterName));

                        RuleFor(x => x.LRNumber)
                            .MaximumLength(maxLengthLRNumber)
                            .WithMessage($"{nameof(CreateInvoiceCommand.LRNumber)} {rule.Error} {maxLengthLRNumber} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.LRNumber));

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateInvoiceCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        // Invoice cannot be created without valid Dispatch Advice
                        RuleFor(x => x.DispatchAdviceId)
                            .MustAsync(async (id, ct) => await _queryRepository.DispatchAdviceExistsAsync(id))
                            .WithMessage($"{nameof(CreateInvoiceCommand.DispatchAdviceId)} {rule.Error}")
                            .When(x => x.DispatchAdviceId > 0);

                        RuleFor(x => x.InvoiceType)
                            .MustAsync(async (id, ct) => await _queryRepository.InvoiceTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateInvoiceCommand.InvoiceType)} {rule.Error}")
                            .When(x => x.InvoiceType > 0);
                        break;

                    case "AlreadyExists":
                        // DA cannot be invoiced twice
                        RuleFor(x => x.DispatchAdviceId)
                            .MustAsync(async (id, ct) => !await _queryRepository.IsAlreadyInvoicedAsync(id))
                            .WithMessage($"Dispatch Advice {rule.Error}")
                            .When(x => x.DispatchAdviceId > 0);
                        break;

                    case "DateCompare":
                        // Invoice date must be >= Dispatch Advice date
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                            {
                                if (cmd.DispatchAdviceId <= 0) return true;
                                var daDate = await _queryRepository.GetDispatchAdviceDateAsync(cmd.DispatchAdviceId);
                                return cmd.InvoiceDate >= daDate;
                            })
                            .WithMessage($"InvoiceDate {rule.Error} Dispatch Advice date.")
                            .When(x => x.DispatchAdviceId > 0);
                        break;

                    case "GreaterThan":
                        // Line-level quantity validations
                        RuleForEach(x => x.Details)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.ItemId)
                                    .GreaterThan(0)
                                    .WithMessage($"ItemId {rule.Error}");

                                detail.RuleFor(d => d.NoOfBags)
                                    .GreaterThan(0)
                                    .WithMessage($"NoOfBags {rule.Error}");

                                detail.RuleFor(d => d.Quantity)
                                    .GreaterThan(0)
                                    .WithMessage($"Quantity {rule.Error}");

                                detail.RuleFor(d => d.RatePerKg)
                                    .GreaterThan(0)
                                    .WithMessage($"RatePerKg {rule.Error}");
                            })
                            .When(x => x.Details != null && x.Details.Any());

                        // Invoice quantity cannot exceed dispatched quantity
                        RuleForEach(x => x.Details!)
                            .MustAsync(async (cmd, detail, ct) =>
                            {
                                var (dispatchedBags, dispatchedQty) = await _queryRepository
                                    .GetDispatchedQuantityAsync(cmd.DispatchAdviceId, detail.ItemId);

                                return detail.NoOfBags <= dispatchedBags && detail.Quantity <= dispatchedQty;
                            })
                            .WithMessage("Invoice quantity cannot exceed dispatched quantity.")
                            .When(x => x.DispatchAdviceId > 0 && x.Details != null && x.Details.Any());
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.Freight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateInvoiceCommand.Freight)} {rule.Error}");

                        RuleFor(x => x.Insurance)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateInvoiceCommand.Insurance)} {rule.Error}");

                        RuleFor(x => x.HandlingCharge)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateInvoiceCommand.HandlingCharge)} {rule.Error}");

                        RuleFor(x => x.OtherCharges)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateInvoiceCommand.OtherCharges)} {rule.Error}");

                        RuleFor(x => x.TCSPercentage)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateInvoiceCommand.TCSPercentage)} {rule.Error}");

                        RuleForEach(x => x.Details)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.Discount)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"Discount {rule.Error}");
                            })
                            .When(x => x.Details != null && x.Details.Any());
                        break;

                    default:
                        break;
                }
            }

            // Final invoice amount must match calculated totals (pure math — no DB call)
            RuleFor(x => x)
                .Must(cmd =>
                {
                    var expectedBeforeTCS = cmd.TaxableValue - cmd.Discount
                        + cmd.Freight + cmd.Insurance + cmd.HandlingCharge + cmd.OtherCharges
                        + cmd.TaxAmount;

                    var expectedFinal = expectedBeforeTCS + cmd.TCS + cmd.RoundOff;

                    const decimal tolerance = 0.01m;
                    return Math.Abs(cmd.InvoiceAmountBeforeTCS - expectedBeforeTCS) <= tolerance
                        && Math.Abs(cmd.InvoiceAmount - expectedFinal) <= tolerance;
                })
                .WithMessage("Final invoice amount does not match calculated totals.");
        }
    }
}
