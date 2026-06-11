using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using FluentValidation;
using PurchaseManagement.Application.Arrival.Commands.UpdateArrival;
using PurchaseManagement.Application.Common.Interfaces.IArrival;

namespace PurchaseManagement.Presentation.Validation.Arrival
{
    public class UpdateArrivalCommandValidator : AbstractValidator<UpdateArrivalCommand>
    {
        public UpdateArrivalCommandValidator(
            IArrivalQueryRepository queryRepo,
            ISupplierLookup supplierLookup,
            IStationLookup stationLookup,
            IWarehouseLookup warehouseLookup,
            ITransporterLookup transporterLookup,
            IItemLookup itemLookup,
            IHSNLookup hsnLookup,
            IPackTypeLookup packTypeLookup,
            IUOMLookup uomLookup)
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Id is required.")
                .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
                .WithMessage("Arrival not found.");

            RuleFor(x => x.RawMaterialPOId)
                .GreaterThan(0).WithMessage("Raw Material PO reference is required.")
                .MustAsync(async (id, ct) => await queryRepo.RawMaterialPOExistsAsync(id))
                .WithMessage("RawMaterialPOId is inactive/deleted.")
                .When(x => x.RawMaterialPOId > 0);

            RuleFor(x => x.VehicleNumber)
                .NotEmpty().WithMessage("Vehicle Number is required.")
                .MaximumLength(30).WithMessage("Vehicle Number cannot be longer than 30 characters.");

            RuleFor(x => x.TareWeight)
                .GreaterThan(0).WithMessage("Tare Weight is required.");

            RuleFor(x => x.GrossWeight)
                .GreaterThan(0).WithMessage("Gross Weight must be greater than zero.");

            RuleFor(x => x.PartyWeight)
                .GreaterThanOrEqualTo(0).WithMessage("Party Weight must be zero or positive.");

            RuleFor(x => x.MoisturePercentage)
                .InclusiveBetween(0m, 100m).WithMessage("Moisture % must be between 0 and 100.")
                .When(x => x.MoisturePercentage.HasValue);

            RuleFor(x => x.SupplierId)
                .GreaterThan(0).WithMessage("Supplier is required.")
                .MustAsync(async (id, ct) => await supplierLookup.GetActiveSupplierByIdAsync(id, ct) != null)
                .WithMessage("SupplierId is inactive/deleted.")
                .When(x => x.SupplierId > 0);

            RuleFor(x => x.StationId)
                .GreaterThan(0).WithMessage("Station is required.")
                .MustAsync(async (id, ct) => (await stationLookup.GetByIdsAsync(new[] { id }, ct)).Any())
                .WithMessage("StationId is inactive/deleted.")
                .When(x => x.StationId > 0);

            RuleFor(x => x.GodownId)
                .GreaterThan(0).WithMessage("Godown is required.")
                .MustAsync(async (id, ct) => (await warehouseLookup.GetByIdsAsync(new[] { id }, ct)).Any())
                .WithMessage("GodownId is inactive/deleted.")
                .When(x => x.GodownId > 0);

            RuleFor(x => x.TransporterId)
                .GreaterThan(0).WithMessage("Transporter is required.")
                .MustAsync(async (id, ct) => await transporterLookup.GetActiveTransporterByIdAsync(id, ct) != null)
                .WithMessage("TransporterId is inactive/deleted.")
                .When(x => x.TransporterId > 0);

            RuleFor(x => x.QcStatusId)
                .GreaterThan(0).WithMessage("QC Status is required.")
                .MustAsync(async (id, ct) => await queryRepo.MiscMasterExistsAsync(id))
                .WithMessage("QcStatusId is inactive/deleted.")
                .When(x => x.QcStatusId > 0);

            RuleFor(x => x.IsActive)
                .InclusiveBetween(0, 1).WithMessage("IsActive must be either 0 or 1.");

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

                d.RuleFor(p => p.PackTypeId)
                    .GreaterThan(0).WithMessage("PackTypeId is required.")
                    .MustAsync(async (id, ct) => (await packTypeLookup.GetByIdsAsync(new[] { id }, ct)).Any())
                    .WithMessage("PackTypeId is inactive/deleted.")
                    .When(p => p.PackTypeId > 0);

                d.RuleFor(p => p.UomId)
                    .GreaterThan(0).WithMessage("UomId is required.")
                    .MustAsync(async (id, ct) => (await uomLookup.GetByIdsAsync(new[] { id }, ct)).Any())
                    .WithMessage("UomId is inactive/deleted.")
                    .When(p => p.UomId > 0);

                d.RuleFor(p => p.MixCodeId).GreaterThan(0).WithMessage("MixCodeId is required.");
                d.RuleFor(p => p.Rate).GreaterThan(0).WithMessage("Rate must be greater than zero.");
                d.RuleFor(p => p.OrderedQty).GreaterThan(0).WithMessage("OrderedQty must be greater than zero.");
                d.RuleFor(p => p.ArrivedQty).GreaterThanOrEqualTo(0).WithMessage("ArrivedQty must be zero or positive.");
                d.RuleFor(p => p.CancelledQty).GreaterThanOrEqualTo(0).WithMessage("CancelledQty must be zero or positive.");

                d.RuleFor(p => p.BaleNumberTo)
                    .GreaterThanOrEqualTo(p => p.BaleNumberFrom)
                    .WithMessage("Bale Number To must be greater than or equal to Bale Number From.");
            });

            // R4 — an arrival line's ArrivedQty may not exceed the PO ordered quantity for that item.
            // Compared per ItemId (summing payload lines that share an item) against the PO quantity.
            RuleFor(x => x).CustomAsync(async (cmd, context, ct) =>
            {
                if (cmd.Details == null || cmd.Details.Count == 0 || cmd.RawMaterialPOId <= 0)
                    return;

                var poQtyByItem = await queryRepo.GetRawMaterialPOItemQuantitiesAsync(cmd.RawMaterialPOId);
                if (poQtyByItem == null || poQtyByItem.Count == 0)
                    return;

                var arrivedByItem = cmd.Details
                    .Where(d => d.ItemId > 0)
                    .GroupBy(d => d.ItemId)
                    .Select(g => new { ItemId = g.Key, Arrived = g.Sum(x => x.ArrivedQty) });

                foreach (var line in arrivedByItem)
                {
                    if (poQtyByItem.TryGetValue(line.ItemId, out var orderedQty) && line.Arrived > orderedQty)
                    {
                        context.AddFailure(
                            $"Arrived quantity ({line.Arrived}) cannot exceed the PO ordered quantity ({orderedQty}) for item {line.ItemId}.");
                    }
                }
            });

            // R3 — within this arrival (one lotno = ArrivalHeader Id), bale numbers may not be duplicated:
            // no two detail lines may have overlapping bale ranges. The arrival's existing bales are fully
            // replaced on save, and bale numbers may repeat across different arrivals — so this is a
            // payload-only check, identical to create.
            RuleFor(x => x).Custom((cmd, context) =>
            {
                if (cmd.Details == null)
                    return;

                var ranges = cmd.Details
                    .Where(l => l.BaleNumberFrom > 0 && l.BaleNumberTo >= l.BaleNumberFrom)
                    .ToList();

                for (var i = 0; i < ranges.Count; i++)
                {
                    for (var j = i + 1; j < ranges.Count; j++)
                    {
                        if (ranges[i].BaleNumberFrom <= ranges[j].BaleNumberTo &&
                            ranges[j].BaleNumberFrom <= ranges[i].BaleNumberTo)
                        {
                            context.AddFailure(
                                $"Bale range {ranges[j].BaleNumberFrom}–{ranges[j].BaleNumberTo} overlaps another line in this arrival.");
                        }
                    }
                }
            });
        }
    }
}
