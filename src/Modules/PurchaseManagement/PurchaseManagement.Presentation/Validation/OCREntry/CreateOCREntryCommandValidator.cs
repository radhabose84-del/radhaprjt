using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.QC;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.CreateOCREntry;

namespace PurchaseManagement.Presentation.Validation.OCREntry
{
    public class CreateOCREntryCommandValidator : AbstractValidator<CreateOCREntryCommand>
    {
        public CreateOCREntryCommandValidator(
            IOCREntryQueryRepository queryRepo,
            ISupplierLookup supplierLookup,
            ILocationMasterLookup locationLookup,
            IStationLookup stationLookup,
            IItemLookup itemLookup,
            ICountMasterLookup countLookup,
            IUOMLookup uomLookup,
            IQualityTemplateLookup qualityTemplateLookup)
        {
            RuleFor(x => x.OcrDate).NotEmpty().WithMessage("OcrDate is required.");

            RuleFor(x => x.ProcurementSourceId)
                .GreaterThan(0).WithMessage("ProcurementSourceId is required.")
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("ProcurementSourceId is inactive/deleted.")
                .When(x => x.ProcurementSourceId > 0);

            RuleFor(x => x.ProcurementTypeId)
                .GreaterThan(0).WithMessage("ProcurementTypeId is required.")
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("ProcurementTypeId is inactive/deleted.")
                .When(x => x.ProcurementTypeId > 0);

            RuleFor(x => x.GradeId!.Value)
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("GradeId is inactive/deleted.")
                .When(x => x.GradeId.HasValue && x.GradeId.Value > 0);

            RuleFor(x => x.PaymentTermId)
                .GreaterThan(0).WithMessage("PaymentTermId is required.")
                .MustAsync(async (id, ct) => await queryRepo.PaymentTermExistsAsync(id))
                .WithMessage("PaymentTermId is inactive/deleted.")
                .When(x => x.PaymentTermId > 0);

            RuleFor(x => x.SupplierId)
                .GreaterThan(0).WithMessage("SupplierId is required.")
                .MustAsync(async (id, ct) => await supplierLookup.GetActiveSupplierOrGinnerByIdAsync(id, ct) != null)
                .WithMessage("SupplierId is inactive/deleted.")
                .When(x => x.SupplierId > 0);

            RuleFor(x => x.LocationId)
                .GreaterThan(0).WithMessage("LocationId is required.")
                .MustAsync(async (id, ct) => await locationLookup.GetByIdAsync(id, ct) != null)
                .WithMessage("LocationId is inactive/deleted.")
                .When(x => x.LocationId > 0);

            RuleFor(x => x.StationId)
                .GreaterThan(0).WithMessage("StationId is required.")
                .MustAsync(async (id, ct) => await stationLookup.GetByIdAsync(id, ct) != null)
                .WithMessage("StationId is inactive/deleted.")
                .When(x => x.StationId > 0);

            RuleFor(x => x.ItemId)
                .GreaterThan(0).WithMessage("ItemId is required.")
                .MustAsync(async (id, ct) => (await itemLookup.GetByIdsAsync(new[] { id }, ct)).Any())
                .WithMessage("ItemId is inactive/deleted.")
                .When(x => x.ItemId > 0);

            RuleFor(x => x.CountId)
                .GreaterThan(0).WithMessage("CountId is required.")
                .MustAsync(async (id, ct) => (await countLookup.GetByIdsAsync(new[] { id }, ct)).Any())
                .WithMessage("CountId is inactive/deleted.")
                .When(x => x.CountId > 0);

            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            RuleFor(x => x.Rate).GreaterThan(0).WithMessage("Rate must be greater than zero.");

            // Composite uniqueness — same OcrDate + ItemId + SupplierId is not allowed.
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                    !await queryRepo.DuplicateOcrExistsAsync(cmd.OcrDate, cmd.ItemId, cmd.SupplierId))
                .WithMessage("An OCR entry with the same date, item and party already exists.")
                .When(x => x.OcrDate != default && x.ItemId > 0 && x.SupplierId > 0);

            // ── Additional Cotton Details — optional MiscMaster FKs ──
            RuleFor(x => x.PaymentModeId!.Value)
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("PaymentModeId is inactive/deleted.")
                .When(x => x.PaymentModeId.HasValue && x.PaymentModeId.Value > 0);

            RuleFor(x => x.UomId!.Value)
                .MustAsync(async (id, ct) => (await uomLookup.GetByIdsAsync(new[] { id }, ct)).Any())
                .WithMessage("UomId is invalid/inactive.")
                .When(x => x.UomId.HasValue && x.UomId.Value > 0);

            RuleFor(x => x.WeighmentId!.Value)
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("WeighmentId is inactive/deleted.")
                .When(x => x.WeighmentId.HasValue && x.WeighmentId.Value > 0);

            RuleFor(x => x.TransitInsuranceId!.Value)
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("TransitInsuranceId is inactive/deleted.")
                .When(x => x.TransitInsuranceId.HasValue && x.TransitInsuranceId.Value > 0);

            RuleFor(x => x.LorryFreightId!.Value)
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("LorryFreightId is inactive/deleted.")
                .When(x => x.LorryFreightId.HasValue && x.LorryFreightId.Value > 0);

            RuleFor(x => x.ModeOfTransportId!.Value)
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("ModeOfTransportId is inactive/deleted.")
                .When(x => x.ModeOfTransportId.HasValue && x.ModeOfTransportId.Value > 0);

            // ── Additional Cotton Details — percentage ranges (0–100) ──
            RuleFor(x => x.DiscountPercentage!.Value)
                .InclusiveBetween(0m, 100m).WithMessage("DiscountPercentage must be between 0 and 100.")
                .When(x => x.DiscountPercentage.HasValue);
            RuleFor(x => x.InsurancePercentage!.Value)
                .InclusiveBetween(0m, 100m).WithMessage("InsurancePercentage must be between 0 and 100.")
                .When(x => x.InsurancePercentage.HasValue);

            // ── Additional Cotton Details — free-text length limits ──
            RuleFor(x => x.MillSampleNo).MaximumLength(50)
                .WithMessage("MillSampleNo cannot be longer than 50 characters.");
            RuleFor(x => x.CottonPassedBy).MaximumLength(100)
                .WithMessage("CottonPassedBy cannot be longer than 100 characters.");
            RuleFor(x => x.Remarks).MaximumLength(500)
                .WithMessage("Remarks cannot be longer than 500 characters.");

            // ── Quality template + dynamic parameters (optional) ──
            RuleFor(x => x.QualityTemplateId!.Value)
                .MustAsync(async (id, ct) => (await qualityTemplateLookup.GetByIdsAsync(new[] { id }, ct)).Any())
                .WithMessage("QualityTemplateId is invalid/inactive.")
                .When(x => x.QualityTemplateId.HasValue && x.QualityTemplateId.Value > 0);

            When(x => x.QualityParameters is { Count: > 0 }, () =>
            {
                RuleForEach(x => x.QualityParameters!).ChildRules(p =>
                {
                    p.RuleFor(d => d.ParamId)
                        .GreaterThan(0).WithMessage("ParamId is required.")
                        .MustAsync(async (id, ct) => (await qualityTemplateLookup.GetParametersByIdsAsync(new[] { id }, ct)).Any())
                        .WithMessage("ParamId is invalid/inactive.")
                        .When(d => d.ParamId > 0);

                    p.RuleFor(d => d.Value).MaximumLength(200)
                        .WithMessage("Value cannot be longer than 200 characters.");
                });
            });
        }
    }
}
