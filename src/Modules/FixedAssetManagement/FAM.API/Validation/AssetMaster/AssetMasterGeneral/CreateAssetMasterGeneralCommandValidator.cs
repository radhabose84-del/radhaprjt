using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.CreateAssetMasterGeneral;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetPurchase;
using FAM.API.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FAM.API.Validation.AssetMaster.AssetMasterGeneral
{
    public class CreateAssetMasterGeneralCommandValidator : AbstractValidator<CreateAssetMasterGeneralCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateAssetMasterGeneralCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            // Max lengths
            var assetMasterGeneralNameMaxLength       = maxLengthProvider.GetMaxLength<AssetMasterGenerals>("AssetName")        ?? 100;
            var assetMasterGeneralMachineCodeMaxLength= maxLengthProvider.GetMaxLength<AssetMasterGenerals>("MachineCode")     ?? 50;
            var assetMasterGeneralDescriptionMaxLength= maxLengthProvider.GetMaxLength<AssetMasterGenerals>("AssetDescription") ?? 1000;

            var BudgetType    = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("BudgetType")    ?? 50;
            var OldUnitIdMax  = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("OldUnitId")     ?? 10;
            var VendorCodeMax = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("VendorCode")    ?? 20;
            var VendorNameMax = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("VendorName")    ?? 200;

            var PoNoMax       = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PoNo")          ?? 10;
            var PoSnoMax      = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PoSno")         ?? 4;
            var ItemCodeMax   = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("ItemCode")      ?? 50;
            var ItemNameMax   = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("ItemName")      ?? 250;
            var GrnNoMax      = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("GrnNo")         ?? 10;
            var GrnSnoMax     = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("GrnSno")        ?? 4;
            var BillNoMax     = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("BillNo")        ?? 50;
            var UomMax        = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("Uom")           ?? 10;
            var BinLocMax     = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("BinLocation")   ?? 50;
            var PjYearMax     = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PjYear")        ?? 8;
            var PjDocIdMax    = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PjDocId")       ?? 20;
            var PjDocSrMax    = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PjDocSr")       ?? 20;
            var PjDocNoMax    = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("PjDocNo")       ?? 10;
            var AssetSourceIdMax = maxLengthProvider.GetMaxLength<AssetPurchaseDetails>("AssetSourceId") ?? 10;

            var JournalNoMax  = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>("JournalNo") ?? 100;
            var CostTypeMax   = maxLengthProvider.GetMaxLength<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>("CostType")  ?? 10;

            _validationRules = ValidationRuleLoader.LoadValidationRules()
                               ?? throw new InvalidOperationException("Validation rules could not be loaded.");

            // ---- Asset master basics ----
            RuleFor(x => x.AssetMaster.AssetName).NotEmpty().MaximumLength(assetMasterGeneralNameMaxLength);
            RuleFor(x => x.AssetMaster.AssetGroupId).GreaterThan(0);
            RuleFor(x => x.AssetMaster.CompanyId).GreaterThan(0);
            RuleFor(x => x.AssetMaster.AssetCategoryId).GreaterThan(0);
            RuleFor(x => x.AssetMaster.AssetSubCategoryId).GreaterThan(0);
            RuleFor(x => x.AssetMaster.Quantity).GreaterThan(0);
            RuleFor(x => x.AssetMaster.UOMId).GreaterThan(0);
            RuleFor(x => x.AssetMaster.AssetDescription).MaximumLength(assetMasterGeneralDescriptionMaxLength);
            RuleFor(x => x.AssetMaster.MachineCode).MaximumLength(assetMasterGeneralMachineCodeMaxLength);

            // ---- Location ----
            RuleFor(x => x.AssetMaster.AssetLocation.UnitId).GreaterThan(0);
            RuleFor(x => x.AssetMaster.AssetLocation.DepartmentId).GreaterThan(0);
            RuleFor(x => x.AssetMaster.AssetLocation.LocationId).GreaterThan(0);
            RuleFor(x => x.AssetMaster.AssetLocation.SubLocationId).GreaterThan(0);

            // ---- Additional cost ----
            RuleForEach(x => x.AssetMaster.AssetAdditionalCost).ChildRules(add =>
            {
                add.RuleFor(x => x.JournalNo).NotEmpty().MaximumLength(JournalNoMax);
                // If CostType must be optional, keep it only for max length when provided:                
                add.RuleFor(x => x.Amount).GreaterThan(0);
            });

            // ---- Purchase details (relaxed for blanks) ----
            RuleForEach(x => x.AssetMaster.AssetPurchaseDetails).ChildRules(p =>
            {
                // Keep OldUnitId required (string, but content up to you)
                p.RuleFor(x => x.OldUnitId).NotEmpty();

                // VendorCode / VendorName OPTIONAL
                p.RuleFor(x => x.VendorCode)
                    .MaximumLength(VendorCodeMax)
                    .When(x => !string.IsNullOrWhiteSpace(x.VendorCode));
                p.RuleFor(x => x.VendorName)
                    .MaximumLength(VendorNameMax)
                    .When(x => !string.IsNullOrWhiteSpace(x.VendorName));

                // ItemCode OPTIONAL (was required earlier)
                p.RuleFor(x => x.ItemCode)
                    .MaximumLength(ItemCodeMax)
                    .When(x => !string.IsNullOrWhiteSpace(x.ItemCode));

                // AcceptedQty can be zero (your sheet has 0.000)
                p.RuleFor(x => x.AcceptedQty).GreaterThanOrEqualTo(0);

                // Either GrnValue or PurchaseValue must be > 0 (keeps economic sanity)
                p.RuleFor(x => x)
                    .Must(x => x.GrnValue > 0 || x.PurchaseValue > 0)
                    .WithMessage("Either GrnValue or PurchaseValue must be greater than 0.");

                // Optional string fields -> validate if provided
                p.RuleFor(x => x.ItemName).MaximumLength(ItemNameMax).When(x => !string.IsNullOrWhiteSpace(x.ItemName));
                p.RuleFor(x => x.BillNo).MaximumLength(BillNoMax).When(x => !string.IsNullOrWhiteSpace(x.BillNo));
                p.RuleFor(x => x.Uom).MaximumLength(UomMax).When(x => !string.IsNullOrWhiteSpace(x.Uom));
                p.RuleFor(x => x.BinLocation).MaximumLength(BinLocMax).When(x => !string.IsNullOrWhiteSpace(x.BinLocation));
                p.RuleFor(x => x.PjYear).MaximumLength(PjYearMax).When(x => !string.IsNullOrWhiteSpace(x.PjYear));
                p.RuleFor(x => x.PjDocId).MaximumLength(PjDocIdMax).When(x => !string.IsNullOrWhiteSpace(x.PjDocId));
                p.RuleFor(x => x.PjDocSr).MaximumLength(PjDocSrMax).When(x => !string.IsNullOrWhiteSpace(x.PjDocSr));

                // Optional ints -> validate only when present (Excel may leave zeros)
                p.RuleFor(x => x.PoNo).GreaterThan(0).When(x => x.PoNo != default);
                p.RuleFor(x => x.PoSno).GreaterThan(0).When(x => x.PoSno != default);
                p.RuleFor(x => x.GrnNo).GreaterThan(0).When(x => x.GrnNo != default);
                p.RuleFor(x => x.GrnSno).GreaterThan(0).When(x => x.GrnSno != default);
                p.RuleFor(x => x.PjDocNo).GreaterThan(0).When(x => x.PjDocNo != default);

                // Y/N when provided
                p.RuleFor(x => x.QcCompleted)
                    .Must(c => !c.HasValue || c is 'Y' or 'N')
                    .WithMessage("QcCompleted must be Y or N when provided.");

                // AssetSourceId only if you truly require it; otherwise validate when present
                p.RuleFor(x => x.AssetSourceId).GreaterThan(0).When(x => x.AssetSourceId != default);
            });

            // (Optional) caps for BudgetType/OldUnitId
            RuleForEach(x => x.AssetMaster.AssetPurchaseDetails).ChildRules(p =>
            {
                p.RuleFor(x => x.BudgetType).MaximumLength(BudgetType).When(x => !string.IsNullOrWhiteSpace(x.BudgetType));
                p.RuleFor(x => x.OldUnitId).MaximumLength(OldUnitIdMax).When(x => !string.IsNullOrWhiteSpace(x.OldUnitId));
            });
        }
    }
}
