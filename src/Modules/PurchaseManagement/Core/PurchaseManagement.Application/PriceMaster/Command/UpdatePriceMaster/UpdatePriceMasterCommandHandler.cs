using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Domain.Entities.PriceMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;

namespace PurchaseManagement.Application.PriceMaster.Commands.Update
{
    public sealed class UpdatePriceMasterCommandHandler : IRequestHandler<UpdatePriceMasterCommand, bool>
    {
        private readonly IPriceMasterCommandRepository _repo;
        private readonly IMapper _mapper;
        private readonly IMiscMasterQueryRepository _miscRepo;

        public UpdatePriceMasterCommandHandler(IPriceMasterCommandRepository repo, IMapper mapper, IMiscMasterQueryRepository miscRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _miscRepo = miscRepo;
        }

        public async Task<bool> Handle(UpdatePriceMasterCommand request, CancellationToken ct)
        {
            var d = request.Data;

            var header = await _repo.LoadAggregateAsync(d.Id, ct)
                        ?? throw new KeyNotFoundException($"PriceMasterHeader {d.Id} not found.");

            // Resolve status ids from MiscMaster (recommended instead of hardcoding 55/56)
            var pending = await _miscRepo.GetMiscMasterByName(
                MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

            var approved = await _miscRepo.GetMiscMasterByName(
                MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);

            // ------------------------------------------------------------
            // CASE 1: APPROVED -> Only allow IsActive to be set to INACTIVE
            // ------------------------------------------------------------
            if (header.StatusId == approved.Id)
            {
                // must be only inactivation
                if (d.IsActive != 0)
                    throw new InvalidOperationException("Approved price masters can only be inactivated (IsActive = 0).");

                // block any scalar changes
                if (header.ItemId != d.ItemId ||
                    header.VendorId != d.VendorId ||
                    header.UomId != d.UomId ||
                    header.ValidFrom != d.ValidFrom ||
                    header.ValidTo != d.ValidTo)
                {
                    throw new InvalidOperationException("Approved price masters cannot change Item/Vendor/UOM/Dates. Only IsActive can be updated to inactive.");
                }

                // Optional: Also prevent details changes. (recommended)
                // Here we allow ONLY changing each existing detail's IsActive to 0,
                // and disallow add/remove/price/currency/qty edits.
               /*  var dtoDetailsById = d.Details
                    .Where(x => x.Id.HasValue)
                    .ToDictionary(x => x.Id!.Value); */

                // Disallow adding new rows or sending rows without id
             /*    if (d.Details.Any(x => !x.Id.HasValue))
                    throw new InvalidOperationException("Approved price masters cannot add new detail rows."); */

                // Disallow removing existing rows
              /*   if (header.Details.Any(x => !dtoDetailsById.ContainsKey(x.Id)))
                    throw new InvalidOperationException("Approved price masters cannot remove detail rows."); */

            /*     foreach (var ent in header.Details)
                {
                    var dto = dtoDetailsById[ent.Id];

                    // disallow edits except IsActive
                    if (ent.ScaleQtyFrom != dto.ScaleQtyFrom ||
                        ent.ScaleQtyTo != dto.ScaleQtyTo ||
                        ent.UnitPrice != dto.UnitPrice ||
                        ent.CurrencyId != dto.CurrencyId)
                    {
                        throw new InvalidOperationException("Approved price masters cannot modify detail values. Only IsActive can be set to inactive.");
                    }

                    // enforce inactive
                    ent.IsActive = BaseEntity.Status.Inactive;
                } */

                // set header inactive
                header.IsActive = BaseEntity.Status.Inactive;

                await _repo.SaveChangesAsync(ct);
                return true;
            }

            // ------------------------------------------------------------
            // CASE 2: PENDING -> Allow full update
            // ------------------------------------------------------------
            if (header.StatusId != pending.Id)
                throw new InvalidOperationException("Only pending price masters can be updated.");

            // Overlap check ONLY when record is being active (or kept active)
            if (d.IsActive == 1)
            {
                if (await _repo.HasOverlappingHeaderExceptAsync(d.Id, d.ItemId, d.VendorId, d.ValidFrom, d.ValidTo, ct))
                    throw new InvalidOperationException("Another ACTIVE PriceMaster overlaps the given validity.");
            }

            var headerStatus = d.IsActive == 1 ? BaseEntity.Status.Active : BaseEntity.Status.Inactive;

            // scalars
            header.ItemId   = d.ItemId;
            header.VendorId = d.VendorId;
            header.ValidFrom = d.ValidFrom;
            header.ValidTo   = d.ValidTo;
            header.IsActive  = headerStatus;
            header.UomId     = d.UomId;

            // details upsert (your existing logic)
            var existingById = header.Details.ToDictionary(x => x.Id);
            var kept = new HashSet<int>();

            foreach (var dto in d.Details
                                .OrderBy(x => x.ScaleQtyFrom)
                                .ThenBy(x => x.ScaleQtyTo ?? decimal.MaxValue))
            {
                if (dto.Id.HasValue && existingById.TryGetValue(dto.Id.Value, out var ent))
                {
                    if (ent.ScaleQtyFrom != dto.ScaleQtyFrom) ent.ScaleQtyFrom = dto.ScaleQtyFrom;
                    if (ent.ScaleQtyTo   != dto.ScaleQtyTo)   ent.ScaleQtyTo   = dto.ScaleQtyTo;
                    if (ent.UnitPrice    != dto.UnitPrice)    ent.UnitPrice    = dto.UnitPrice;
                    if (ent.CurrencyId   != dto.CurrencyId)   ent.CurrencyId   = dto.CurrencyId;

                    var newStatus = dto.IsActive == 1 ? BaseEntity.Status.Active : BaseEntity.Status.Inactive;
                    if (ent.IsActive != newStatus) ent.IsActive = newStatus;

                    kept.Add(ent.Id);
                }
                else
                {
                    var newDetail = _mapper.Map<PriceMasterDetail>(dto);
                    header.Details.Add(newDetail);
                }
            }

            var toDelete = header.Details
                                .Where(x => existingById.ContainsKey(x.Id) && !kept.Contains(x.Id))
                                .ToList();
            foreach (var del in toDelete)
                header.Details.Remove(del);

            await _repo.SaveChangesAsync(ct);
            return true;
        }

        // public async Task<bool> Handle(UpdatePriceMasterCommand request, CancellationToken ct)
        // {
        //     var d = request.Data;

        //     var header = await _repo.LoadAggregateAsync(d.Id, ct)
        //                  ?? throw new KeyNotFoundException($"PriceMasterHeader {d.Id} not found.");

        //     var statusId = await _miscRepo.GetMiscMasterByName(
        //             MiscEnumEntity.ApprovalStatus,
        //             MiscEnumEntity.Pending);

        //     if (header.StatusId != statusId.Id)
        //         throw new InvalidOperationException("Approved price masters cannot be updated.");

        //     if (await _repo.HasOverlappingHeaderExceptAsync(d.Id, d.ItemId, d.VendorId, d.ValidFrom, d.ValidTo, ct))
        //         throw new InvalidOperationException("Another PriceMaster overlaps the given validity.");

        //     var status = d.IsActive == 1
        //     ? BaseEntity.Status.Active
        //     : BaseEntity.Status.Inactive;
        //     // scalars
        //     header.ItemId        = d.ItemId;
        //     header.VendorId      = d.VendorId;            
        //     header.ValidFrom     = d.ValidFrom;
        //     header.ValidTo       = d.ValidTo;                        
        //     header.IsActive      = status;
        //     header.UomId         = d.UomId;

        //     var existingById = header.Details.ToDictionary(x => x.Id);
        //     var kept = new HashSet<int>();

        //     foreach (var dto in d.Details
        //                          .OrderBy(x => x.ScaleQtyFrom)
        //                          .ThenBy(x => x.ScaleQtyTo ?? decimal.MaxValue))
        //     {
        //         if (dto.Id.HasValue && existingById.TryGetValue(dto.Id.Value, out var ent))
        //         {
        //             // update only when value actually differs
        //             if (ent.ScaleQtyFrom != dto.ScaleQtyFrom) ent.ScaleQtyFrom = dto.ScaleQtyFrom;
        //             if (ent.ScaleQtyTo   != dto.ScaleQtyTo)   ent.ScaleQtyTo   = dto.ScaleQtyTo;
        //             if (ent.UnitPrice    != dto.UnitPrice)    ent.UnitPrice    = dto.UnitPrice;      
        //             if (ent.CurrencyId    != dto.CurrencyId)    ent.CurrencyId    = dto.CurrencyId;      
        //             var newStatus = dto.IsActive == 1
        //                 ? BaseEntity.Status.Active
        //                 : BaseEntity.Status.Inactive;

        //             if (ent.IsActive != newStatus)
        //                 ent.IsActive = newStatus;   
        //             kept.Add(ent.Id);
        //         }
        //         else
        //         {
        //             // new row
        //             var newDetail = _mapper.Map<PriceMasterDetail>(dto);
        //             header.Details.Add(newDetail);
        //         }
        //     }

        //     // delete rows that are no longer present in payload
        //     var toDelete = header.Details
        //                          .Where(x => existingById.ContainsKey(x.Id) && !kept.Contains(x.Id))
        //                          .ToList();
        //     foreach (var del in toDelete)
        //         header.Details.Remove(del);

        //     await _repo.SaveChangesAsync(ct);
        //     return true;
        // }
    }
}
