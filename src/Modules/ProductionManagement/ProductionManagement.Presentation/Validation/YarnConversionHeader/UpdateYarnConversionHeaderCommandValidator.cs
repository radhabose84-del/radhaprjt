using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;
using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Commands.UpdateYarnConversionHeader;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.YarnConversionHeader
{
    public class UpdateYarnConversionHeaderCommandValidator : AbstractValidator<UpdateYarnConversionHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IYarnConversionHeaderQueryRepository _queryRepo;
        private readonly IItemLookup _itemLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;

        public UpdateYarnConversionHeaderCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IYarnConversionHeaderQueryRepository queryRepo,
            IItemLookup itemLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup)
        {
            _queryRepo = queryRepo;
            _itemLookup = itemLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup = binLookup;

            var maxLengthRemarks     = maxLengthProvider.GetMaxLength<Domain.Entities.YarnConversionHeader>("Remarks")     ?? 500;
            var maxLengthWasteReason = maxLengthProvider.GetMaxLength<Domain.Entities.YarnConversionHeader>("WasteReason") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Yarn Conversion {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.LotId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.LotId)} {rule.Error}");

                        RuleFor(x => x.OldItemId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldItemId)} {rule.Error}");

                        RuleFor(x => x.OldPackTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldPackTypeId)} {rule.Error}");

                        RuleFor(x => x.OldStartPackNo)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldStartPackNo)} {rule.Error}");

                        RuleFor(x => x.OldEndPackNo)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldEndPackNo)} {rule.Error}");

                        RuleFor(x => x.OldTotalBags)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldTotalBags)} {rule.Error}");

                        RuleFor(x => x.OldWarehouseId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldWarehouseId)} {rule.Error}");

                        RuleFor(x => x.OldBinId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldBinId)} {rule.Error}");

                        RuleFor(x => x.ItemId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.ItemId)} {rule.Error}");

                        RuleFor(x => x.PackTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.PackTypeId)} {rule.Error}");

                        RuleFor(x => x.TotalBags)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.TotalBags)} {rule.Error}");

                        RuleFor(x => x.WarehouseId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.WarehouseId)} {rule.Error}");

                        RuleFor(x => x.BinId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.BinId)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.OldNetWeightPerPack)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldNetWeightPerPack)} {rule.Error}");

                        RuleFor(x => x.OldNetWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldNetWeight)} {rule.Error}");

                        RuleFor(x => x.NetWeightPerPack)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.NetWeightPerPack)} {rule.Error}");

                        RuleFor(x => x.NetWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.NetWeight)} {rule.Error}");

                        RuleFor(x => x.LooseQty)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.LooseQty)} {rule.Error}");

                        RuleFor(x => x.WasteQty)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.WasteQty)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));

                        RuleFor(x => x.WasteReason)
                            .MaximumLength(maxLengthWasteReason)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.WasteReason)} {rule.Error} {maxLengthWasteReason} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.WasteReason));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.LotId)
                            .MustAsync(async (id, ct) => await _queryRepo.LotMasterExistsAsync(id))
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.LotId)} {rule.Error}")
                            .When(x => x.LotId > 0);

                        RuleFor(x => x.OldItemId)
                            .MustAsync(async (id, ct) =>
                            {
                                var items = await _itemLookup.GetByIdsAsync(new[] { id }, ct);
                                return items.Any();
                            })
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldItemId)} {rule.Error}")
                            .When(x => x.OldItemId > 0);

                        RuleFor(x => x.OldPackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldPackTypeId)} {rule.Error}")
                            .When(x => x.OldPackTypeId > 0);

                        RuleFor(x => x.OldWarehouseId)
                            .MustAsync(async (id, ct) =>
                            {
                                var whs = await _warehouseLookup.GetByIdsAsync(new[] { id }, ct);
                                return whs.Any();
                            })
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldWarehouseId)} {rule.Error}")
                            .When(x => x.OldWarehouseId > 0);

                        RuleFor(x => x.OldBinId)
                            .MustAsync(async (id, ct) =>
                            {
                                var bins = await _binLookup.GetByIdsAsync(new[] { id }, ct);
                                return bins.Any();
                            })
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldBinId)} {rule.Error}")
                            .When(x => x.OldBinId > 0);

                        RuleFor(x => x.FaultId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.FaultId)} {rule.Error}")
                            .When(x => x.FaultId.HasValue && x.FaultId > 0);

                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) =>
                            {
                                var items = await _itemLookup.GetByIdsAsync(new[] { id }, ct);
                                return items.Any();
                            })
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.ItemId)} {rule.Error}")
                            .When(x => x.ItemId > 0);

                        RuleFor(x => x.PackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.PackTypeId)} {rule.Error}")
                            .When(x => x.PackTypeId > 0);

                        RuleFor(x => x.LooseHandlingId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.LooseHandlingId)} {rule.Error}")
                            .When(x => x.LooseHandlingId.HasValue && x.LooseHandlingId > 0);

                        RuleFor(x => x.WarehouseId)
                            .MustAsync(async (id, ct) =>
                            {
                                var whs = await _warehouseLookup.GetByIdsAsync(new[] { id }, ct);
                                return whs.Any();
                            })
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.WarehouseId)} {rule.Error}")
                            .When(x => x.WarehouseId > 0);

                        RuleFor(x => x.BinId)
                            .MustAsync(async (id, ct) =>
                            {
                                var bins = await _binLookup.GetByIdsAsync(new[] { id }, ct);
                                return bins.Any();
                            })
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.BinId)} {rule.Error}")
                            .When(x => x.BinId > 0);

                        RuleFor(x => x.WasteTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.WasteTypeId)} {rule.Error}")
                            .When(x => x.WasteTypeId.HasValue && x.WasteTypeId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Custom: OldEndPackNo >= OldStartPackNo
            RuleFor(x => x.OldEndPackNo)
                .GreaterThanOrEqualTo(x => x.OldStartPackNo)
                .WithMessage("OldEndPackNo must be greater than or equal to OldStartPackNo.")
                .When(x => x.OldStartPackNo > 0 && x.OldEndPackNo > 0);
        }
    }
}
