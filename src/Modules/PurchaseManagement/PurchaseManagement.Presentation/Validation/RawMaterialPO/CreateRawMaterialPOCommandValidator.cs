using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.CreateRawMaterialPO;

namespace PurchaseManagement.Presentation.Validation.RawMaterialPO
{
    public class CreateRawMaterialPOCommandValidator : AbstractValidator<CreateRawMaterialPOCommand>
    {
        public CreateRawMaterialPOCommandValidator(
            IRawMaterialPOQueryRepository queryRepo,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup)
        {
            // R1 / R2 — only Approved OCRs are convertible
            RuleFor(x => x.OcrId)
                .GreaterThan(0).WithMessage("OCR reference is required.")
                .MustAsync(async (id, ct) => await queryRepo.OcrExistsAndApprovedAsync(id))
                .WithMessage("Only approved OCRs can be converted.")
                .When(x => x.OcrId > 0);

            RuleFor(x => x.ProcurementDocumentTypeId)
                .GreaterThan(0).WithMessage("Procurement Document Type is required.")
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("ProcurementDocumentTypeId is inactive/deleted.")
                .When(x => x.ProcurementDocumentTypeId > 0);

            // Additional cotton details (all optional)
            RuleFor(x => x.CropYear)
                .MaximumLength(20).WithMessage("CropYear cannot be longer than 20 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.CropYear));

            RuleFor(x => x.ArrivalType)
                .MaximumLength(50).WithMessage("ArrivalType cannot be longer than 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.ArrivalType));

            RuleFor(x => x.CottonApprovedBy)
                .MaximumLength(100).WithMessage("CottonApprovedBy cannot be longer than 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.CottonApprovedBy));

            RuleFor(x => x.CreditDays)
                .GreaterThanOrEqualTo(0).WithMessage("CreditDays must be zero or positive.")
                .When(x => x.CreditDays.HasValue);

            RuleFor(x => x.DocumentPath)
                .MaximumLength(500).WithMessage("DocumentPath cannot be longer than 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.DocumentPath));

            RuleFor(x => x.Details)
                .NotEmpty().WithMessage("At least one detail line is required.");

            RuleForEach(x => x.Details).ChildRules(d =>
            {
                d.RuleFor(p => p.ItemId)
                    .GreaterThan(0).WithMessage("ItemId is required.")
                    .MustAsync(async (id, ct) => (await itemLookup.GetByIdsAsync(new[] { id }, ct)).Any())
                    .WithMessage("ItemId is inactive/deleted.")
                    .When(p => p.ItemId > 0);

                d.RuleFor(p => p.HsnId)
                    .GreaterThan(0).WithMessage("HsnId is required.")
                    .MustAsync(async (id, ct) => (await hsnLookup.GetByIdsAsync(new[] { id }, ct)).Any())
                    .WithMessage("HsnId is inactive/deleted.")
                    .When(p => p.HsnId > 0);

                d.RuleFor(p => p.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
                d.RuleFor(p => p.Rate).GreaterThan(0).WithMessage("Rate must be greater than zero.");
                d.RuleFor(p => p.Weight).GreaterThanOrEqualTo(0).WithMessage("Weight must be zero or positive.")
                    .When(p => p.Weight.HasValue);
            });

            // Partial-conversion cap — Σ(new bales) + already-converted must not exceed the OCR total
            RuleFor(x => x).CustomAsync(async (cmd, context, ct) =>
            {
                if (cmd.OcrId <= 0 || cmd.Details == null || cmd.Details.Count == 0)
                    return;

                var ocrQty = await queryRepo.GetOcrQuantityAsync(cmd.OcrId);
                var already = await queryRepo.GetConvertedQuantityAsync(cmd.OcrId, null);
                var requested = cmd.Details.Sum(p => p.Quantity);

                if (already + requested > ocrQty)
                    context.AddFailure(
                        $"Quantity to convert exceeds the remaining OCR balance ({ocrQty - already} bales).");
            });
        }
    }
}
