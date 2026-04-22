using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Commands.UpdateInvoice;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.Invoice
{
    public class UpdateInvoiceCommandValidator : AbstractValidator<UpdateInvoiceCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IInvoiceQueryRepository _queryRepository;        

        public UpdateInvoiceCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IInvoiceQueryRepository queryRepository            )
        {
            _queryRepository = queryRepository;            

            var maxLengthVehicleNumber   = maxLengthProvider.GetMaxLength<Domain.Entities.InvoiceHeader>("VehicleNumber")   ?? 20;
            var maxLengthTransporterName = maxLengthProvider.GetMaxLength<Domain.Entities.InvoiceHeader>("TransporterName") ?? 100;
            var maxLengthLRNumber        = maxLengthProvider.GetMaxLength<Domain.Entities.InvoiceHeader>("LRNumber")        ?? 50;
            var maxLengthRemarks         = maxLengthProvider.GetMaxLength<Domain.Entities.InvoiceHeader>("Remarks")         ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateInvoiceCommand.Id)} {rule.Error}");

                        RuleFor(x => x.Details)
                            .NotNull()
                            .WithMessage($"Details {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"Details {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.VehicleNumber)
                            .MaximumLength(maxLengthVehicleNumber)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.VehicleNumber)} {rule.Error} {maxLengthVehicleNumber} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.VehicleNumber));

                        RuleFor(x => x.TransporterName)
                            .MaximumLength(maxLengthTransporterName)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.TransporterName)} {rule.Error} {maxLengthTransporterName} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.TransporterName));

                        RuleFor(x => x.LRNumber)
                            .MaximumLength(maxLengthLRNumber)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.LRNumber)} {rule.Error} {maxLengthLRNumber} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.LRNumber));

                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        // InvoiceTypeId must reference a valid MiscMaster record
                        RuleFor(x => x.InvoiceTypeId)
                            .MustAsync(async (id, ct) => await _queryRepository.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateInvoiceCommand.InvoiceTypeId)} {rule.Error}")
                            .When(x => x.InvoiceTypeId.HasValue && x.InvoiceTypeId > 0);
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0)
                            .WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Invoice {rule.Error}")
                            .MustAsync(async (id, ct) => await _queryRepository.IsInvoicePendingAsync(id))
                            .WithMessage("Only invoices with 'Pending' status can be updated.");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.IsActive)} {rule.Error}");
                        break;

                    case "GreaterThan":
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
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.TotalDiscount)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.TotalDiscount)} {rule.Error}");

                        RuleFor(x => x.TotalFreight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.TotalFreight)} {rule.Error}");

                        RuleFor(x => x.TotalCommission)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.TotalCommission)} {rule.Error}");

                        RuleFor(x => x.Insurance)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.Insurance)} {rule.Error}");

                        RuleFor(x => x.HandlingCharge)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.HandlingCharge)} {rule.Error}");

                        RuleFor(x => x.TotalCharity)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.TotalCharity)} {rule.Error}");

                        RuleFor(x => x.OtherCharges)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.OtherCharges)} {rule.Error}");

                        RuleFor(x => x.TCSPercentage)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateInvoiceCommand.TCSPercentage)} {rule.Error}");

                        RuleForEach(x => x.Details)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.DiscountValue)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"DiscountValue {rule.Error}");

                                detail.RuleFor(d => d.FreightValue)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"FreightValue {rule.Error}");

                                detail.RuleFor(d => d.CommissionValue)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"CommissionValue {rule.Error}");

                                detail.RuleFor(d => d.Charity)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"Charity {rule.Error}");

                                detail.RuleFor(d => d.HandlingCharges)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"HandlingCharges {rule.Error}");
                            })
                            .When(x => x.Details != null && x.Details.Any());
                        break;

                    default:
                        break;
                }
            }

       /*      // Final invoice amount must match calculated totals
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
                .WithMessage("Final invoice amount does not match calculated totals."); */
        }
    }
}
