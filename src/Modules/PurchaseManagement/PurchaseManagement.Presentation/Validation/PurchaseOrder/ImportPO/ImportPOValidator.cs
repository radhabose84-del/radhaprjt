using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
using FluentValidation;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Validation
{
    // ---------------------------
    // Command-level validators
    // ---------------------------
 
    public sealed class CreateImportPOCommandValidator : AbstractValidator<CreateImportPOCommand>
    {
        public CreateImportPOCommandValidator()
        {
            RuleFor(x => x.Data).NotNull();
            RuleFor(x => x.Data).SetValidator(new ImportPOCreateDtoValidator());
        }
    }

    public sealed class UpdateImportPOCommandValidator : AbstractValidator<UpdateImportPOCommand>
    {
        public UpdateImportPOCommandValidator()
        {
            RuleFor(x => x.Data).NotNull();
            RuleFor(x => x.Data.Id).GreaterThan(0);
            RuleFor(x => x.Data).SetValidator(new ImportPOCreateDtoValidator());
        }
    }

    // ---------------------------
    // DTO-level validators
    // ---------------------------

    public sealed class ImportPOCreateDtoValidator : AbstractValidator<ImportPOCreateDto>
    {
        public ImportPOCreateDtoValidator()
        {
            RuleFor(x => x.VendorId).GreaterThan(0);
            RuleFor(x => x.CurrencyId).GreaterThan(0);

            // Collections must exist and have items
            RuleFor(x => x.Headers)
                .NotNull().WithMessage("At least one Import header is required.")
                .Must(h => h.Any()).WithMessage("At least one Import header is required.");

            RuleForEach(x => x.Headers).SetValidator(new ImportPOHeaderDtoValidator());

            RuleFor(x => x.PaymentTerms)
                .NotNull().WithMessage("Payment terms collection cannot be null.");

            RuleForEach(x => x.PaymentTerms).SetValidator(new PurchasePaymentTermDtoValidator());

            // Basic monetary sanity (PO totals are computed elsewhere but must be non-negative)
            RuleFor(x => x.ItemTotal).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DiscountTotal).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PandFTotal).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MiscCharges).GreaterThanOrEqualTo(0);
            RuleFor(x => x.GSTTotal).GreaterThanOrEqualTo(0);
            RuleFor(x => x.FreightTotal).GreaterThanOrEqualTo(0);
            RuleFor(x => x.InsuranceTotal).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TDSTotal).GreaterThanOrEqualTo(0);
            RuleFor(x => x.AdvanceAmount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PurchaseValue).GreaterThanOrEqualTo(0);
        }
    }

    public sealed class ImportPOHeaderDtoValidator : AbstractValidator<ImportPOHeaderDto>
    {
        public ImportPOHeaderDtoValidator()
        {            
            RuleFor(x => x.IncotermId).GreaterThan(0);

            // Critical logistics fields
            RuleFor(x => x.ExpectedTimeOfDeparture).NotEmpty();
            RuleFor(x => x.ExpectedTimeOfArrival).NotEmpty();
            RuleFor(x => x.ExpectedTimeOfArrival)
                .Must((h, eta) => eta >= h.ExpectedTimeOfDeparture)
                .WithMessage("ETA must be greater than or equal to ETD.");

            RuleFor(x => x.BillOfEntryNumber).MaximumLength(80);

            // Optional identifiers + sane lengths
            RuleFor(x => x.BillOfLadingNumber).MaximumLength(80);
            RuleFor(x => x.AirWaybillNumber).MaximumLength(40);
            RuleFor(x => x.ContainerNumber).MaximumLength(40);
            RuleFor(x => x.VesselName).MaximumLength(120);
            RuleFor(x => x.AirlineName).MaximumLength(120);
            RuleFor(x => x.FlightNumber).MaximumLength(40);
            RuleFor(x => x.DemurrageTerms).MaximumLength(512);
            RuleFor(x => x.TTReferenceNumber).MaximumLength(80);
            RuleFor(x => x.TTRemarks).MaximumLength(1024);
            RuleFor(x => x.LCRemarks).MaximumLength(1024);

            // Conditional dependencies:
            // If AWB is present, AirlineName should be present.
            When(x => !string.IsNullOrWhiteSpace(x.AirWaybillNumber), () =>
            {
                RuleFor(x => x.AirlineName)
                    .NotEmpty().WithMessage("AirlineName is required when AirWaybillNumber is provided.");
            });

            // If BL is present, VesselName should be present.
            When(x => !string.IsNullOrWhiteSpace(x.BillOfLadingNumber), () =>
            {
                RuleFor(x => x.VesselName)
                    .NotEmpty().WithMessage("VesselName is required when BillOfLadingNumber is provided.");
            });

            // LC constraints (if provided)
            RuleFor(x => x.LCAmount).GreaterThanOrEqualTo(0).When(x => x.LCAmount.HasValue);
            When(x => x.LCDate.HasValue && x.LCExpiryDate.HasValue, () =>
            {
                RuleFor(x => x.LCExpiryDate!.Value)
                    .Must((h, expiry) => expiry >= h.LCDate)
                    .WithMessage("LCExpiryDate must be on/after LCDate.");
            });

            // Bank/PaymentStatus FK sanity (optional fields if present)
            RuleFor(x => x.LCIssueBankId).GreaterThan(0).When(x => x.LCIssueBankId.HasValue);
            RuleFor(x => x.LCBeneficiaryBankId).GreaterThan(0).When(x => x.LCBeneficiaryBankId.HasValue);
            RuleFor(x => x.LCTypeId).GreaterThan(0).When(x => x.LCTypeId.HasValue);
            RuleFor(x => x.TTBankId).GreaterThan(0).When(x => x.TTBankId.HasValue);
            RuleFor(x => x.TTPaymentStatusId).GreaterThan(0).When(x => x.TTPaymentStatusId.HasValue);
            RuleFor(x => x.TTPaymentModeId).GreaterThan(0).When(x => x.TTPaymentModeId.HasValue);
            RuleFor(x => x.LCPaymentModeId).GreaterThan(0).When(x => x.LCPaymentModeId.HasValue);
            RuleFor(x => x.LCPaymentStatusId).GreaterThan(0).When(x => x.LCPaymentStatusId.HasValue);

            // Details required
            RuleFor(x => x.Details)
                .NotNull().WithMessage("At least one detail row is required.")
                .Must(d => d.Any()).WithMessage("At least one detail row is required.");
            RuleForEach(x => x.Details).SetValidator(new ImportPODetailDtoValidator());
        }
    }

    public sealed class ImportPODetailDtoValidator : AbstractValidator<ImportPODetailDto>
    {
        public ImportPODetailDtoValidator()
        {
            RuleFor(x => x.IndentId).GreaterThan(0);
            RuleFor(x => x.ItemId).GreaterThan(0);
            RuleFor(x => x.UomId).GreaterThan(0);

            // Money/amounts non-negative
            RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
            RuleFor(x => x.FreightAmount).GreaterThanOrEqualTo(0).When(x => x.AgriInfraDevCess.HasValue);
            RuleFor(x => x.InsuranceAmount).GreaterThanOrEqualTo(0).When(x => x.AgriInfraDevCess.HasValue);
            RuleFor(x => x.CIFValue).GreaterThanOrEqualTo(0).When(x => x.AgriInfraDevCess.HasValue);
            RuleFor(x => x.BasicCustomDuty).GreaterThanOrEqualTo(0).When(x => x.AgriInfraDevCess.HasValue);
            RuleFor(x => x.SocialWelfareSurCharges).GreaterThanOrEqualTo(0).When(x => x.AgriInfraDevCess.HasValue);
            RuleFor(x => x.IGST).GreaterThanOrEqualTo(0).When(x => x.AgriInfraDevCess.HasValue);
            RuleFor(x => x.AgriInfraDevCess).GreaterThanOrEqualTo(0).When(x => x.AgriInfraDevCess.HasValue);
            RuleFor(x => x.AntiDumpingDuty).GreaterThanOrEqualTo(0).When(x => x.AntiDumpingDuty.HasValue);
            RuleFor(x => x.SafeguardDuty).GreaterThanOrEqualTo(0).When(x => x.SafeguardDuty.HasValue);
            RuleFor(x => x.HealthEducationCess).GreaterThanOrEqualTo(0).When(x => x.HealthEducationCess.HasValue);
            RuleFor(x => x.OtherCharges).GreaterThanOrEqualTo(0).When(x => x.OtherCharges.HasValue);
            RuleFor(x => x.TotalValue).GreaterThanOrEqualTo(0);
        }
    }

    public sealed class PurchasePaymentTermDtoValidator : AbstractValidator<ImportPurchasePaymentTermDto>
    {
        public PurchasePaymentTermDtoValidator()
        {
            RuleFor(x => x.PaymentTermId).GreaterThan(0);

            // optional but if provided must be sensible
            RuleFor(x => x.AdvancePercent).InclusiveBetween(0, 100).When(x => x.AdvancePercent.HasValue);
            RuleFor(x => x.BalancePercent).InclusiveBetween(0, 100).When(x => x.BalancePercent.HasValue);

            RuleFor(x => x.CreditDays).GreaterThanOrEqualTo(0).When(x => x.CreditDays.HasValue);
            RuleFor(x => x.InsurancePercent).InclusiveBetween(0, 100).When(x => x.InsurancePercent.HasValue);

            RuleFor(x => x.AdvanceAmount).GreaterThanOrEqualTo(0).When(x => x.AdvanceAmount.HasValue);
            RuleFor(x => x.BalanceAmount).GreaterThanOrEqualTo(0).When(x => x.BalanceAmount.HasValue);
            RuleFor(x => x.InsuranceAmount).GreaterThanOrEqualTo(0).When(x => x.InsuranceAmount.HasValue);

            RuleFor(x => x.PaymentModelId).GreaterThan(0).When(x => x.PaymentModelId.HasValue);
            RuleFor(x => x.InsuranceId).GreaterThan(0).When(x => x.InsuranceId.HasValue);
        }
    } 
}
