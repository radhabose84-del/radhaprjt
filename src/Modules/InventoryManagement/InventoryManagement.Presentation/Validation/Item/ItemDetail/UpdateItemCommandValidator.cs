#nullable disable
using System.Linq;
using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Commands.UpdateItem;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems; // DTOs
using InventoryManagement.Application.Common.Text;
using InventoryManagement.Presentation.Validation.Common;

namespace InventoryManagement.Presentation.Validation.Item.ItemDetail
{
    public sealed class UpdateItemCommandValidator : AbstractValidator<UpdateItemCommand>
    {
        public UpdateItemCommandValidator(
            IItemCommandRepository itemRepo,
            IItemQueryRepository qryRepo,
            IMaxLengthProvider maxLenProvider,
            IValidator<ItemPurchaseDto> purchaseV,
            IValidator<ItemInventoryDto> inventoryV,
            IValidator<ItemQualityDto> qualityV,
            IValidator<ItemSupplierDto> supplierRowV,
            IValidator<ItemManufactureDto> manuRowV,
            IValidator<ItemUomDto> uomRowV)
        #pragma warning disable CS0618
        {
        #pragma warning restore CS0618
            #pragma warning disable CS0618
            CascadeMode = CascadeMode.Stop;
            #pragma warning restore CS0618

            int codeMax = 50, nameMax = 200;
            try
            {
                codeMax = maxLenProvider.GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemMaster>(
                              nameof(InventoryManagement.Domain.Entities.Item.ItemDetail.ItemMaster.ItemCode)) ?? 50;
                nameMax = maxLenProvider.GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemMaster>(
                              nameof(InventoryManagement.Domain.Entities.Item.ItemDetail.ItemMaster.ItemName)) ?? 200;
            }
            catch { }

            RuleFor(x => x.Payload.Id)
                .GreaterThan(0).WithMessage("Item Id is required.");

            RuleFor(x => x.Payload.ItemName)
                .NotEmpty().WithMessage("ItemName is required.")
                .MaximumLength(nameMax);
            
            RuleFor(x => x.Payload.ItemCode)
                .MaximumLength(codeMax)
                .When(x => !string.IsNullOrWhiteSpace(x.Payload.ItemCode));

            RuleFor(x => x.Payload.ItemCode)
                .MustAsync(async (cmd, code, ct) =>
                {
                    if (string.IsNullOrWhiteSpace(code)) return true;
                    try { return !await itemRepo.ExistsByCodeForUpdateAsync(code, cmd.Payload.Id, ct); }
                    catch { return true; }
                })
                .WithMessage("Another item with the same ItemCode exists.")
                .When(x => !string.IsNullOrWhiteSpace(x.Payload.ItemCode));

            RuleFor(x => x.Payload.ItemName)
                .MustAsync(async (cmd, name, ct) =>
                {
                    try { return !await itemRepo.ExistsByNameSmartForUpdateAsync(name, cmd.Payload.Id, ct); }
                    catch { return true; }
                })
                .WithMessage("Item name already exists.");

            RuleFor(x => x.Payload.ItemName)
                .MustAsync(async (cmd, name, ct) =>
                {
                    try
                    {
                        var norm = NameSimilarity.Normalize(name);
                        var candidates = await qryRepo.GetCandidateItemNamesAsync(norm, 200, ct);
                        if (candidates is null || candidates.Count == 0) return true;
                        return !NameSimilarity.IsTooSimilarToAny(norm, candidates);
                    }
                    catch { return true; }
                })
                .WithMessage("This name is highly similar to an existing item. Please choose a more distinct name.");

            When(x => x.Payload.Purchase is { } p && !IsEmptyPurchase(p),
                () => RuleFor(x => x.Payload.Purchase!).SetValidator(purchaseV));

            When(x => x.Payload.Inventory is { } inv && !IsEmptyInventory(inv),
                () => RuleFor(x => x.Payload.Inventory!).SetValidator(inventoryV));

            When(x => x.Payload.Quality is { } q && !IsEmptyQuality(q),
                () => RuleFor(x => x.Payload.Quality!).SetValidator(qualityV));

            RuleForEach(x => x.Payload.Suppliers)
                .Where(r => !(r.SupplierId == 0 && r.UnitId == 0 && string.IsNullOrWhiteSpace(r.SupplierPartNo)))
                .SetValidator(supplierRowV);

            RuleForEach(x => x.Payload.Manufacture)
                .Where(r => !(r.UnitId == 0 && r.ManufacturingTypeId == 0))
                .SetValidator(manuRowV);

            RuleForEach(x => x.Payload.Uoms)
                .Where(r => r.ConversionUOMId.HasValue || r.ConversionRate.HasValue)
                .SetValidator(uomRowV);

            RuleFor(x => x.Payload.Suppliers)
                .Must(list => list.Select(s => (s.SupplierId, s.UnitId)).Distinct().Count() == list.Count)
                .WithMessage("Duplicate Supplier+Unit rows are not allowed.");

            RuleFor(x => x.Payload.Manufacture)
                .Must(list => list.Select(m => (m.UnitId, m.ManufacturingTypeId)).Distinct().Count() == list.Count)
                .WithMessage("Duplicate Unit+ManufacturingType rows are not allowed.");

            RuleFor(x => x.Payload.Uoms)
                .Must(list =>
                {
                    var withId = list.Where(u => u.ConversionUOMId.HasValue).ToList();
                    return withId.Select(u => u.ConversionUOMId!.Value).Distinct().Count() == withId.Count;
                })
                .WithMessage("Duplicate ConversionUOM not allowed for the same Item.");
        }

        private static bool IsEmptyPurchase(ItemPurchaseDto p) =>
            !p.PurchaseUomId.HasValue && !p.LeadTimeDays.HasValue &&
            !p.SafetyStock.HasValue && !p.GrProcessingTimeDays.HasValue &&
            !p.OriginCountryId.HasValue && string.IsNullOrWhiteSpace(p.TariffNumber);

        private static bool IsEmptyInventory(ItemInventoryDto p) =>
            !p.Weight.HasValue && !p.WeightUomId.HasValue &&
            !p.DefaultMaterialRequestTypeId.HasValue && !p.ValuationMethodId.HasValue &&
            !p.ShelfLife.HasValue && !p.UpperTolerance.HasValue && !p.LowerTolerance.HasValue &&
            string.IsNullOrWhiteSpace(p.BatchNumberSeries) && string.IsNullOrWhiteSpace(p.SerialNumberSeries) &&
            !p.ReorderLevel.HasValue && !p.ReorderQty.HasValue && !p.RequestTypeId.HasValue &&
            !p.AllowNegativeStock && !p.BatchManagement && !p.ApplyBatchNumber;

        private static bool IsEmptyQuality(ItemQualityDto p) =>
            !p.InspectionTemplateId.HasValue && !p.CertificateTypeId.HasValue &&
            !p.InspLotProcessingTime.HasValue && !p.InspectionRequired &&
            !p.QualityInspectionFree && !p.IsCertificateRequiredFromSupplier;
    }
}
