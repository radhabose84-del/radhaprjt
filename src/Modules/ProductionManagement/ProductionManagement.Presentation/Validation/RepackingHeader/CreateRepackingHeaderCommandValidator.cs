using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;
using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.CreateRepackingHeader;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.RepackingHeader
{
    public class CreateRepackingHeaderCommandValidator : AbstractValidator<CreateRepackingHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IRepackingHeaderQueryRepository _queryRepo;
        private readonly IItemLookup _itemLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;

        public CreateRepackingHeaderCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IRepackingHeaderQueryRepository queryRepo,
            IItemLookup itemLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup)
        {
            _queryRepo = queryRepo;
            _itemLookup = itemLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup = binLookup;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.RepackingHeader>("Remarks") ?? 500;
            var maxLengthWasteReason = maxLengthProvider.GetMaxLength<Domain.Entities.RepackingHeader>("WasteReason") ?? 500;

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
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.ItemId)} {rule.Error}");
                        RuleFor(x => x.OldItemId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.OldItemId)} {rule.Error}");
                        RuleFor(x => x.PackTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.PackTypeId)} {rule.Error}");
                        RuleFor(x => x.OldPackTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.OldPackTypeId)} {rule.Error}");
                        RuleFor(x => x.TotalBags)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.TotalBags)} {rule.Error}");
                        RuleFor(x => x.WarehouseId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.WarehouseId)} {rule.Error}");
                        RuleFor(x => x.BinId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.BinId)} {rule.Error}");

                        // Detail-level validation
                        RuleForEach(x => x.Details).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.OldStartPackNo)
                                .GreaterThan(0)
                                .WithMessage($"{nameof(CreateRepackingDetailItem.OldStartPackNo)} {rule.Error}");
                            detail.RuleFor(d => d.OldEndPackNo)
                                .GreaterThan(0)
                                .WithMessage($"{nameof(CreateRepackingDetailItem.OldEndPackNo)} {rule.Error}");
                            detail.RuleFor(d => d.OldTotalBags)
                                .GreaterThan(0)
                                .WithMessage($"{nameof(CreateRepackingDetailItem.OldTotalBags)} {rule.Error}");
                            detail.RuleFor(d => d.OldWarehouseId)
                                .GreaterThan(0)
                                .WithMessage($"{nameof(CreateRepackingDetailItem.OldWarehouseId)} {rule.Error}");
                            detail.RuleFor(d => d.OldBinId)
                                .GreaterThan(0)
                                .WithMessage($"{nameof(CreateRepackingDetailItem.OldBinId)} {rule.Error}");
                        });
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.NetWeightPerPack)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.NetWeightPerPack)} {rule.Error}");
                        RuleFor(x => x.NetWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.NetWeight)} {rule.Error}");
                        RuleFor(x => x.LooseConeKgs)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.LooseConeKgs)} {rule.Error}");
                        RuleFor(x => x.WasteQuantity)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.WasteQuantity)} {rule.Error}");

                        // Detail-level
                        RuleForEach(x => x.Details).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.OldNetWeightPerPack)
                                .GreaterThanOrEqualTo(0)
                                .WithMessage($"{nameof(CreateRepackingDetailItem.OldNetWeightPerPack)} {rule.Error}");
                            detail.RuleFor(d => d.OldNetWeight)
                                .GreaterThanOrEqualTo(0)
                                .WithMessage($"{nameof(CreateRepackingDetailItem.OldNetWeight)} {rule.Error}");
                        });
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        RuleFor(x => x.WasteReason)
                            .MaximumLength(maxLengthWasteReason)
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.WasteReason)} {rule.Error} {maxLengthWasteReason} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.WasteReason));
                        break;

                    case "FKColumnDelete":
                        // Same-module FKs
                        RuleFor(x => x.PackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.PackTypeId)} {rule.Error}")
                            .When(x => x.PackTypeId > 0);
                        RuleFor(x => x.OldPackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.OldPackTypeId)} {rule.Error}")
                            .When(x => x.OldPackTypeId > 0);
                        RuleFor(x => x.LooseHandlingId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.LooseHandlingId)} {rule.Error}")
                            .When(x => x.LooseHandlingId.HasValue && x.LooseHandlingId > 0);
                        RuleFor(x => x.FaultId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.FaultId)} {rule.Error}")
                            .When(x => x.FaultId.HasValue && x.FaultId > 0);
                        RuleFor(x => x.WasteTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.WasteTypeId)} {rule.Error}")
                            .When(x => x.WasteTypeId.HasValue && x.WasteTypeId > 0);
                        RuleFor(x => x.LotId)
                            .MustAsync(async (id, ct) => await _queryRepo.LotMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.LotId)} {rule.Error}")
                            .When(x => x.LotId.HasValue && x.LotId > 0);

                        // Cross-module FKs
                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) =>
                            {
                                var items = await _itemLookup.GetByIdsAsync(new[] { id }, ct);
                                return items.Count > 0;
                            })
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.ItemId)} {rule.Error}")
                            .When(x => x.ItemId > 0);
                        RuleFor(x => x.OldItemId)
                            .MustAsync(async (id, ct) =>
                            {
                                var items = await _itemLookup.GetByIdsAsync(new[] { id }, ct);
                                return items.Count > 0;
                            })
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.OldItemId)} {rule.Error}")
                            .When(x => x.OldItemId > 0);
                        RuleFor(x => x.WarehouseId)
                            .MustAsync(async (id, ct) =>
                            {
                                var wh = await _warehouseLookup.GetByIdsAsync(new[] { id }, ct);
                                return wh.Count > 0;
                            })
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.WarehouseId)} {rule.Error}")
                            .When(x => x.WarehouseId > 0);
                        RuleFor(x => x.BinId)
                            .MustAsync(async (id, ct) =>
                            {
                                var bins = await _binLookup.GetByIdsAsync(new[] { id }, ct);
                                return bins.Count > 0;
                            })
                            .WithMessage($"{nameof(CreateRepackingHeaderCommand.BinId)} {rule.Error}")
                            .When(x => x.BinId > 0);

                        // Detail-level cross-module FKs
                        RuleForEach(x => x.Details).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.OldWarehouseId)
                                .MustAsync(async (id, ct) =>
                                {
                                    var wh = await _warehouseLookup.GetByIdsAsync(new[] { id }, ct);
                                    return wh.Count > 0;
                                })
                                .WithMessage($"{nameof(CreateRepackingDetailItem.OldWarehouseId)} {rule.Error}")
                                .When(d => d.OldWarehouseId > 0);
                            detail.RuleFor(d => d.OldBinId)
                                .MustAsync(async (id, ct) =>
                                {
                                    var bins = await _binLookup.GetByIdsAsync(new[] { id }, ct);
                                    return bins.Count > 0;
                                })
                                .WithMessage($"{nameof(CreateRepackingDetailItem.OldBinId)} {rule.Error}")
                                .When(d => d.OldBinId > 0);
                        });
                        break;

                    default:
                        break;
                }
            }

            // Custom: OldEndPackNo >= OldStartPackNo per detail
            RuleForEach(x => x.Details).ChildRules(detail =>
            {
                detail.RuleFor(d => d.OldEndPackNo)
                    .GreaterThanOrEqualTo(d => d.OldStartPackNo)
                    .WithMessage("OldEndPackNo must be greater than or equal to OldStartPackNo.");
            });

            // At least one detail row required
            RuleFor(x => x.Details)
                .NotEmpty()
                .WithMessage("At least one detail row is required.");
        }
    }
}
