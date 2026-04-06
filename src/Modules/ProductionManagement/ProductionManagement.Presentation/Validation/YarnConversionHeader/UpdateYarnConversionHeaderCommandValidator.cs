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

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.YarnConversionHeader>("Remarks") ?? 500;

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
                            .WithMessage($"YarnConversionHeader {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.LotId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.LotId)} {rule.Error}");
                        RuleFor(x => x.ItemId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.ItemId)} {rule.Error}");
                        RuleFor(x => x.OldItemId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldItemId)} {rule.Error}");
                        RuleFor(x => x.PackTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.PackTypeId)} {rule.Error}");
                        RuleFor(x => x.OldPackTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldPackTypeId)} {rule.Error}");
                        RuleFor(x => x.TotalBags)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.TotalBags)} {rule.Error}");
                        RuleFor(x => x.WarehouseId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.WarehouseId)} {rule.Error}");
                        RuleFor(x => x.OldWarehouseId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldWarehouseId)} {rule.Error}");
                        RuleFor(x => x.BinId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.BinId)} {rule.Error}");
                        RuleFor(x => x.OldBinId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldBinId)} {rule.Error}");
                        RuleFor(x => x.NetWeightPerPack)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.NetWeightPerPack)} {rule.Error}");
                        RuleFor(x => x.NetWeight)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.NetWeight)} {rule.Error}");
                        RuleFor(x => x.OldNetWeightPerPack)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldNetWeightPerPack)} {rule.Error}");
                        RuleFor(x => x.OldTotalBags)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldTotalBags)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.OldNetWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldNetWeight)} {rule.Error}");
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
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.LotId)
                            .MustAsync(async (id, ct) => await _queryRepo.LotMasterExistsAsync(id))
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.LotId)} {rule.Error}")
                            .When(x => x.LotId > 0);

                        RuleFor(x => x.PackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.PackTypeId)} {rule.Error}")
                            .When(x => x.PackTypeId > 0);

                        RuleFor(x => x.OldPackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldPackTypeId)} {rule.Error}")
                            .When(x => x.OldPackTypeId > 0);

                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) =>
                            {
                                var items = await _itemLookup.GetByIdsAsync(new[] { id }, ct);
                                return items.Any(i => i.Id == id);
                            })
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.ItemId)} {rule.Error}")
                            .When(x => x.ItemId > 0);

                        RuleFor(x => x.WarehouseId)
                            .MustAsync(async (id, ct) =>
                            {
                                var warehouses = await _warehouseLookup.GetByIdsAsync(new[] { id }, ct);
                                return warehouses.Any(w => w.Id == id);
                            })
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.WarehouseId)} {rule.Error}")
                            .When(x => x.WarehouseId > 0);

                        RuleFor(x => x.BinId)
                            .MustAsync(async (id, ct) =>
                            {
                                var bins = await _binLookup.GetByIdsAsync(new[] { id }, ct);
                                return bins.Any(b => b.Id == id);
                            })
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.BinId)} {rule.Error}")
                            .When(x => x.BinId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.IsActive)} {rule.Error}");
                        break;

                    case "DateCompare":
                        RuleFor(x => x.OldEndPackNo)
                            .GreaterThanOrEqualTo(x => x.OldStartPackNo)
                            .WithMessage($"{nameof(UpdateYarnConversionHeaderCommand.OldEndPackNo)} must be greater than or equal to {nameof(UpdateYarnConversionHeaderCommand.OldStartPackNo)}.");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
