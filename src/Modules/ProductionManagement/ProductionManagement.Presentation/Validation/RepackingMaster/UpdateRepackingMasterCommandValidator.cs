using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;
using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Commands.UpdateRepackingMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.RepackingMaster
{
    public class UpdateRepackingMasterCommandValidator : AbstractValidator<UpdateRepackingMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IRepackingMasterQueryRepository _queryRepo;
        private readonly IItemLookup _itemLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;

        public UpdateRepackingMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IRepackingMasterQueryRepository queryRepo,
            IItemLookup itemLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup)
        {
            _queryRepo = queryRepo;
            _itemLookup = itemLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup = binLookup;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.RepackingMaster>("Remarks") ?? 500;

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
                            .WithMessage($"Repacking Master {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.ItemId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.ItemId)} {rule.Error}");

                        RuleFor(x => x.OldPackTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.OldPackTypeId)} {rule.Error}");

                        RuleFor(x => x.OldStartPackNo)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.OldStartPackNo)} {rule.Error}");

                        RuleFor(x => x.OldEndPackNo)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.OldEndPackNo)} {rule.Error}");

                        RuleFor(x => x.OldTotalBags)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.OldTotalBags)} {rule.Error}");

                        RuleFor(x => x.PackTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.PackTypeId)} {rule.Error}");

                        RuleFor(x => x.TotalBags)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.TotalBags)} {rule.Error}");

                        RuleFor(x => x.OldWarehouseId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.OldWarehouseId)} {rule.Error}");

                        RuleFor(x => x.OldBinId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.OldBinId)} {rule.Error}");

                        RuleFor(x => x.WarehouseId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.WarehouseId)} {rule.Error}");

                        RuleFor(x => x.BinId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.BinId)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.OldNetWeightPerPack)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.OldNetWeightPerPack)} {rule.Error}");

                        RuleFor(x => x.OldNetWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.OldNetWeight)} {rule.Error}");

                        RuleFor(x => x.NetWeightPerPack)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.NetWeightPerPack)} {rule.Error}");

                        RuleFor(x => x.NetWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.NetWeight)} {rule.Error}");

                        RuleFor(x => x.LooseConeKgs)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.LooseConeKgs)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) =>
                            {
                                var items = await _itemLookup.GetByIdsAsync(new[] { id }, ct);
                                return items.Any();
                            })
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.ItemId)} {rule.Error}")
                            .When(x => x.ItemId > 0);

                        RuleFor(x => x.OldPackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.OldPackTypeId)} {rule.Error}")
                            .When(x => x.OldPackTypeId > 0);

                        RuleFor(x => x.PackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.PackTypeId)} {rule.Error}")
                            .When(x => x.PackTypeId > 0);

                        RuleFor(x => x.SelectionModeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.SelectionModeId)} {rule.Error}")
                            .When(x => x.SelectionModeId.HasValue && x.SelectionModeId > 0);

                        RuleFor(x => x.LooseHandlingId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.LooseHandlingId)} {rule.Error}")
                            .When(x => x.LooseHandlingId.HasValue && x.LooseHandlingId > 0);

                        RuleFor(x => x.OldWarehouseId)
                            .MustAsync(async (id, ct) =>
                            {
                                var whs = await _warehouseLookup.GetByIdsAsync(new[] { id }, ct);
                                return whs.Any();
                            })
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.OldWarehouseId)} {rule.Error}")
                            .When(x => x.OldWarehouseId > 0);

                        RuleFor(x => x.WarehouseId)
                            .MustAsync(async (id, ct) =>
                            {
                                var whs = await _warehouseLookup.GetByIdsAsync(new[] { id }, ct);
                                return whs.Any();
                            })
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.WarehouseId)} {rule.Error}")
                            .When(x => x.WarehouseId > 0);

                        RuleFor(x => x.OldBinId)
                            .MustAsync(async (id, ct) =>
                            {
                                var bins = await _binLookup.GetByIdsAsync(new[] { id }, ct);
                                return bins.Any();
                            })
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.OldBinId)} {rule.Error}")
                            .When(x => x.OldBinId > 0);

                        RuleFor(x => x.BinId)
                            .MustAsync(async (id, ct) =>
                            {
                                var bins = await _binLookup.GetByIdsAsync(new[] { id }, ct);
                                return bins.Any();
                            })
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.BinId)} {rule.Error}")
                            .When(x => x.BinId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateRepackingMasterCommand.IsActive)} {rule.Error}");
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
