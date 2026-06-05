using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Application.RawMaterialPO.Commands.UpdateRawMaterialPO;

namespace PurchaseManagement.Presentation.Validation.RawMaterialPO
{
    public class UpdateRawMaterialPOCommandValidator : AbstractValidator<UpdateRawMaterialPOCommand>
    {
        public UpdateRawMaterialPOCommandValidator(
            IRawMaterialPOQueryRepository queryRepo,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup)
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Id is required.")
                .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
                .WithMessage("Raw Material PO not found.")
                .When(x => x.Id > 0);

            RuleFor(x => x.ProcurementDocumentTypeId)
                .GreaterThan(0).WithMessage("Procurement Document Type is required.")
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("ProcurementDocumentTypeId is inactive/deleted.")
                .When(x => x.ProcurementDocumentTypeId > 0);

            RuleFor(x => x.IsActive)
                .InclusiveBetween(0, 1).WithMessage("IsActive must be either 0 or 1.");

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

            // Partial-conversion cap (excluding this header's own previously-converted bales)
            RuleFor(x => x).CustomAsync(async (cmd, context, ct) =>
            {
                if (cmd.Id <= 0 || cmd.Details == null || cmd.Details.Count == 0)
                    return;

                var existing = await queryRepo.GetByIdAsync(cmd.Id);
                if (existing == null)
                    return;

                var ocrQty = await queryRepo.GetOcrQuantityAsync(existing.OcrId);
                var otherConverted = await queryRepo.GetConvertedQuantityAsync(existing.OcrId, cmd.Id);
                var requested = cmd.Details.Sum(p => p.Quantity);

                if (otherConverted + requested > ocrQty)
                    context.AddFailure(
                        $"Quantity to convert exceeds the remaining OCR balance ({ocrQty - otherConverted} bales).");
            });
        }
    }
}
