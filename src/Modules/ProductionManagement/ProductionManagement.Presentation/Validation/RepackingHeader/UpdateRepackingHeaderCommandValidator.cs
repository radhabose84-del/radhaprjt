using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Warehouse;
using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.CreateRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.UpdateRepackingHeader;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.RepackingHeader
{
    public class UpdateRepackingHeaderCommandValidator : AbstractValidator<UpdateRepackingHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IRepackingHeaderQueryRepository _queryRepo;
        private readonly IItemLookup _itemLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;

        public UpdateRepackingHeaderCommandValidator(
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
                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"RepackingHeader {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.ItemId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.ItemId)} {rule.Error}");
                        RuleFor(x => x.OldItemId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.OldItemId)} {rule.Error}");
                        RuleFor(x => x.PackTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.PackTypeId)} {rule.Error}");
                        RuleFor(x => x.OldPackTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.OldPackTypeId)} {rule.Error}");
                        RuleFor(x => x.TotalBags)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.TotalBags)} {rule.Error}");
                        RuleFor(x => x.WarehouseId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.WarehouseId)} {rule.Error}");
                        RuleFor(x => x.BinId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.BinId)} {rule.Error}");

                        RuleForEach(x => x.Details).ChildRules(detail =>
                        {
                            detail.RuleFor(d => d.OldStartPackNo)
                                .GreaterThan(0)
                                .WithMessage($"{nameof(CreateRepackingDetailItem.OldStartPackNo)} {rule.Error}");
                            detail.RuleFor(d => d.OldEndPackNo)
                                .GreaterThan(0)
                                .WithMessage($"{nameof(CreateRepackingDetailItem.OldEndPackNo)} {rule.Error}");
                        });
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.NetWeightPerPack)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.NetWeightPerPack)} {rule.Error}");
                        RuleFor(x => x.NetWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.NetWeight)} {rule.Error}");
                        RuleFor(x => x.LooseConeKgs)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.LooseConeKgs)} {rule.Error}");
                        RuleFor(x => x.WasteQuantity)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.WasteQuantity)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        RuleFor(x => x.WasteReason)
                            .MaximumLength(maxLengthWasteReason)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.WasteReason)} {rule.Error} {maxLengthWasteReason} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.WasteReason));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.PackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.PackTypeId)} {rule.Error}")
                            .When(x => x.PackTypeId > 0);
                        RuleFor(x => x.OldPackTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.OldPackTypeId)} {rule.Error}")
                            .When(x => x.OldPackTypeId > 0);
                        RuleFor(x => x.LooseHandlingId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.LooseHandlingId)} {rule.Error}")
                            .When(x => x.LooseHandlingId.HasValue && x.LooseHandlingId > 0);
                        RuleFor(x => x.FaultId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.FaultId)} {rule.Error}")
                            .When(x => x.FaultId.HasValue && x.FaultId > 0);
                        RuleFor(x => x.WasteTypeId)
                            .MustAsync(async (id, ct) => await _queryRepo.MiscMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.WasteTypeId)} {rule.Error}")
                            .When(x => x.WasteTypeId.HasValue && x.WasteTypeId > 0);
                        RuleFor(x => x.LotId)
                            .MustAsync(async (id, ct) => await _queryRepo.LotMasterExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.LotId)} {rule.Error}")
                            .When(x => x.LotId.HasValue && x.LotId > 0);
                        RuleFor(x => x.ItemId)
                            .MustAsync(async (id, ct) =>
                            {
                                var items = await _itemLookup.GetByIdsAsync(new[] { id }, ct);
                                return items.Count > 0;
                            })
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.ItemId)} {rule.Error}")
                            .When(x => x.ItemId > 0);
                        RuleFor(x => x.OldItemId)
                            .MustAsync(async (id, ct) =>
                            {
                                var items = await _itemLookup.GetByIdsAsync(new[] { id }, ct);
                                return items.Count > 0;
                            })
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.OldItemId)} {rule.Error}")
                            .When(x => x.OldItemId > 0);
                        RuleFor(x => x.WarehouseId)
                            .MustAsync(async (id, ct) =>
                            {
                                var wh = await _warehouseLookup.GetByIdsAsync(new[] { id }, ct);
                                return wh.Count > 0;
                            })
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.WarehouseId)} {rule.Error}")
                            .When(x => x.WarehouseId > 0);
                        RuleFor(x => x.BinId)
                            .MustAsync(async (id, ct) =>
                            {
                                var bins = await _binLookup.GetByIdsAsync(new[] { id }, ct);
                                return bins.Count > 0;
                            })
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.BinId)} {rule.Error}")
                            .When(x => x.BinId > 0);

                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateRepackingHeaderCommand.IsActive)} {rule.Error}");
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

            RuleFor(x => x.Details)
                .NotEmpty()
                .WithMessage("At least one detail row is required.");
        }
    }
}
