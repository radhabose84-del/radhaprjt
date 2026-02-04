using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Item.ItemDetail.Commands.CreateItem;
using InventoryManagement.API.Validation.Common;
using InventoryManagement.Application.Common.Text;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using Shared.Validation.Common;

namespace InventoryManagement.API.Validation.Item.ItemDetail
{
    public sealed class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
    {
        public CreateItemCommandValidator(
            IItemCommandRepository itemRepo,
            IMaxLengthProvider maxLenProvider,
            IValidator<ItemPurchaseDto> purchaseV,
            IValidator<ItemInventoryDto> inventoryV,
            IValidator<ItemQualityDto> qualityV,
            IValidator<ItemSupplierDto> supplierRowV,
            IValidator<ItemManufactureDto> manuRowV,
            IValidator<ItemUomDto> uomRowV,
            IItemQueryRepository qryRepo)
        {
            CascadeMode = CascadeMode.Stop;

            // Safe max lengths
            int codeMax = 50, nameMax = 200;
            try
            {
                codeMax = maxLenProvider.GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemMaster>(
                              nameof(InventoryManagement.Domain.Entities.Item.ItemDetail.ItemMaster.ItemCode)) ?? 50;
                nameMax = maxLenProvider.GetMaxLength<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemMaster>(
                              nameof(InventoryManagement.Domain.Entities.Item.ItemDetail.ItemMaster.ItemName)) ?? 200;
            }
            catch { /* defaults */ }

            var rules = ValidationRuleLoader.LoadValidationRules() ?? new List<ValidationRule>();
            foreach (var rule in rules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Payload.ItemName)
                            .NotEmpty().WithMessage($"{nameof(CreateItemCommand.Payload.ItemName)} {rule.Error}");
                       /*  RuleFor(x => x.Payload.UnitId)
                            .GreaterThan(0).WithMessage($"{nameof(CreateItemCommand.Payload.UnitId)} {rule.Error}"); */
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Payload.ItemName)
                            .MaximumLength(nameMax).WithMessage($"{nameof(CreateItemCommand.Payload.ItemName)} {rule.Error}");
                        // If you ever accept client-supplied ItemCode, re-enable:
                        // RuleFor(x => x.Payload.ItemCode).MaximumLength(codeMax)...
                        break;

                    case "AlreadyExists":
                        // Skip uniqueness when creating children from a template
                        RuleFor(x => x.Payload.ItemName)
                          .MustAsync(async (cmd, name, ct) =>
                          {
                              // Skip uniqueness when creating children from a template
                              if (cmd.Payload.ParentItemId.HasValue) return true;

                              try { return !await itemRepo.ExistsByNameSmartForCreateAsync(name, ct); }
                              catch { return true; } // fail-open on infra issues
                          })
                          .WithMessage("Item name already exists.");
                        break;
                }
            }

            // Tabs: validate only when non-empty
            When(x => x.Payload.Purchase is { } p && !IsEmptyPurchase(p),
                () => RuleFor(x => x.Payload.Purchase!).SetValidator(purchaseV));

            When(x => x.Payload.Inventory is { } inv && !IsEmptyInventory(inv),
                () => RuleFor(x => x.Payload.Inventory!).SetValidator(inventoryV));

            When(x => x.Payload.Quality is { } q && !IsEmptyQuality(q),
                () => RuleFor(x => x.Payload.Quality!).SetValidator(qualityV));

            // Lists: skip blank rows, then validate
            RuleForEach(x => x.Payload.Suppliers)
                .Where(r => !(r.SupplierId == 0 && r.UnitId == 0 && string.IsNullOrWhiteSpace(r.SupplierPartNo)))
                .SetValidator(supplierRowV);

            RuleForEach(x => x.Payload.Manufacture)
                .Where(r => !(r.UnitId == 0 && r.ManufacturingTypeId == 0))
                .SetValidator(manuRowV);

            RuleForEach(x => x.Payload.Uoms)
                .Where(r => r.ConversionUOMId.HasValue || r.ConversionRate.HasValue)
                .SetValidator(uomRowV);

            // Duplicate prevention
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
            RuleFor(x => x.Payload.Purchase!.LeadTimeDays)
                .InclusiveBetween(0, 3650) // example: 0..10 years
                .When(x => x.Payload?.Purchase?.LeadTimeDays is not null);

            // “Too similar” check (best-effort)
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
                    catch
                    {
                        return true; // ignore repo errors
                    }
                })
                .WithMessage("This name is highly similar to an existing item. Please choose a more distinct name.");

            // Extra: if creating a template (HasVariants && no ParentItemId) require attributes
            When(x => x.Payload.HasVariants && !x.Payload.ParentItemId.HasValue, () =>
            {
                RuleFor(x => x.Payload.VariantAttributes)
                    .NotEmpty().WithMessage("Variant attributes are required for a template.");

                RuleForEach(x => x.Payload.VariantAttributes).ChildRules(a =>
                {
                    a.RuleFor(v => v.AttributeId).GreaterThan(0);
                    a.RuleFor(v => v.VariantBasedOn).GreaterThan(0);
                    a.RuleFor(v => v.Order).GreaterThan(0);
                });

                RuleFor(x => x.Payload.VariantAttributes.Select(a => a.AttributeId))
                    .Must(ids => ids.Distinct().Count() == ids.Count())
                    .WithMessage("Duplicate attributes are not allowed.");

                RuleFor(x => x.Payload.VariantAttributes.Select(a => a.Order))
                    .Must(ord => ord.Distinct().Count() == ord.Count())
                    .WithMessage("Attribute order must be unique.");
            });
        }

        // ---- helpers ----
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
