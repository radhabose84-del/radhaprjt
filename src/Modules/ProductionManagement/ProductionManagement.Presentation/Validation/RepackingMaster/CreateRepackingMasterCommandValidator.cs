using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;
using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Commands.CreateRepackingMaster;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.RepackingMaster
{
    public class CreateRepackingMasterCommandValidator : AbstractValidator<CreateRepackingMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IRepackingMasterQueryRepository _queryRepo;
        private readonly IItemLookup _itemLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;

        public CreateRepackingMasterCommandValidator(
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
                    case "GreaterThan":
                        RuleFor(x => x.ItemId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.ItemId)} {rule.Error}");

                        RuleFor(x => x.OldPackTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.OldPackTypeId)} {rule.Error}");

                        RuleFor(x => x.OldStartPackNo)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.OldStartPackNo)} {rule.Error}");

                        RuleFor(x => x.OldEndPackNo)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.OldEndPackNo)} {rule.Error}");

                        RuleFor(x => x.OldTotalBags)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.OldTotalBags)} {rule.Error}");

                        RuleFor(x => x.PackTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.PackTypeId)} {rule.Error}");

                        RuleFor(x => x.TotalBags)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.TotalBags)} {rule.Error}");

                        RuleFor(x => x.OldWarehouseId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.OldWarehouseId)} {rule.Error}");

                        RuleFor(x => x.OldBinId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.OldBinId)} {rule.Error}");

                        RuleFor(x => x.WarehouseId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.WarehouseId)} {rule.Error}");

                        RuleFor(x => x.BinId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.BinId)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.OldNetWeightPerPack)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.OldNetWeightPerPack)} {rule.Error}");

                        RuleFor(x => x.OldNetWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.OldNetWeight)} {rule.Error}");

                        RuleFor(x => x.NetWeightPerPack)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.NetWeightPerPack)} {rule.Error}");

                        RuleFor(x => x.NetWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.NetWeight)} {rule.Error}");

                        RuleFor(x => x.LooseConeKgs)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.LooseConeKgs)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) =>
                            {
                                var items = await _itemLookup.GetByIdsAsync(new[] { id }, ct);
                                return items.Any();
                            })
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.ItemId)} {rule.Error}")
                            .When(x => x.ItemId > 0);

                        RuleFor(x => x.OldPackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.OldPackTypeId)} {rule.Error}")
                            .When(x => x.OldPackTypeId > 0);

                        RuleFor(x => x.PackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.PackTypeId)} {rule.Error}")
                            .When(x => x.PackTypeId > 0);

                        RuleFor(x => x.SelectionModeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.SelectionModeId)} {rule.Error}")
                            .When(x => x.SelectionModeId.HasValue && x.SelectionModeId > 0);

                        RuleFor(x => x.LooseHandlingId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.LooseHandlingId)} {rule.Error}")
                            .When(x => x.LooseHandlingId.HasValue && x.LooseHandlingId > 0);

                        RuleFor(x => x.OldWarehouseId)
                            .MustAsync(async (id, ct) =>
                            {
                                var whs = await _warehouseLookup.GetByIdsAsync(new[] { id }, ct);
                                return whs.Any();
                            })
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.OldWarehouseId)} {rule.Error}")
                            .When(x => x.OldWarehouseId > 0);

                        RuleFor(x => x.WarehouseId)
                            .MustAsync(async (id, ct) =>
                            {
                                var whs = await _warehouseLookup.GetByIdsAsync(new[] { id }, ct);
                                return whs.Any();
                            })
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.WarehouseId)} {rule.Error}")
                            .When(x => x.WarehouseId > 0);

                        RuleFor(x => x.OldBinId)
                            .MustAsync(async (id, ct) =>
                            {
                                var bins = await _binLookup.GetByIdsAsync(new[] { id }, ct);
                                return bins.Any();
                            })
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.OldBinId)} {rule.Error}")
                            .When(x => x.OldBinId > 0);

                        RuleFor(x => x.BinId)
                            .MustAsync(async (id, ct) =>
                            {
                                var bins = await _binLookup.GetByIdsAsync(new[] { id }, ct);
                                return bins.Any();
                            })
                            .WithMessage($"{nameof(CreateRepackingMasterCommand.BinId)} {rule.Error}")
                            .When(x => x.BinId > 0);
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
