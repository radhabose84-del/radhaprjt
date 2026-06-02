using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Gate;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingHeader
{
    public class GetGrnPendingHeaderQueryHandler : IRequestHandler<GetGrnPendingHeaderQuery, ApiResponseDTO<List<GetGrnPendingHeaderDto>>>
    {
        private readonly IGRNEntryQueryRepository _iGrnEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IPartyLookup _partyLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IGateInwardLookup _gateInwardLookup;

        public GetGrnPendingHeaderQueryHandler(IGRNEntryQueryRepository iGrnEntryQueryRepository, IMapper mapper, IMediator mediator,
        IPartyLookup partyLookup, IWarehouseLookup warehouseLookup, IGateInwardLookup gateInwardLookup)
        {
            _iGrnEntryQueryRepository = iGrnEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _partyLookup = partyLookup;
            _warehouseLookup = warehouseLookup;
            _gateInwardLookup = gateInwardLookup;
        }

        

        public async  Task<ApiResponseDTO<List<GetGrnPendingHeaderDto>>> Handle(GetGrnPendingHeaderQuery request, CancellationToken cancellationToken)
        {
           var (result, totalCount) = await _iGrnEntryQueryRepository.GetPendingGrnHeaderAsync(
            request.FromDate,
            request.ToDate,
            request.IsGrnGenerated,
            request.IsQcGenerated,
            request.PageNumber,
            request.PageSize,
            request.SearchTerm
        );

        var pendingGrnList = _mapper.Map<List<GetGrnPendingHeaderDto>>(result);

        // Collect unique Party and Warehouse IDs
        var partyIds = pendingGrnList
            .Where(x => x.PartyId > 0)
            .Select(x => x.PartyId)
            .Distinct()
            .ToList();

        var warehouseIds = pendingGrnList
            .Where(x => (x.ReceivingWarehouseId.HasValue && x.ReceivingWarehouseId > 0)
                    || (x.QcWarehouseId.HasValue && x.QcWarehouseId > 0))
            .Select(x => x.ReceivingWarehouseId ?? x.QcWarehouseId!.Value)
            .Distinct()
            .ToList();

        // Fire gRPC calls concurrently (batching by Id collection)
        var partyLookupResult = partyIds.Any()
            ? await _partyLookup.GetByIdsAsync(partyIds, cancellationToken)
            : Array.Empty<Contracts.Dtos.Lookups.Party.PartyLookupDto>() as IReadOnlyList<Contracts.Dtos.Lookups.Party.PartyLookupDto>;

        var warehouseLookupResult = warehouseIds.Any()
            ? await _warehouseLookup.GetByIdsAsync(warehouseIds, cancellationToken)
            : Array.Empty<Contracts.Dtos.Lookups.Warehouse.WarehouseLookupDto>() as IReadOnlyList<Contracts.Dtos.Lookups.Warehouse.WarehouseLookupDto>;

        var partyResults = partyLookupResult.ToDictionary(p => p.Id, p => p);
        var warehouseResults = warehouseLookupResult.ToDictionary(w => w.Id, w => w);

        // ── Cross-module: resolve GateEntryNo + GateEntryDate via the Gate lookup ──
        // GateEntryId on the GRN row points at Gate.GateInwardHdr.Id (centralized flow).
        // Replaces the dropped INNER JOIN to the legacy Purchase.GateEntryHeader table.
        var gateEntryIds = pendingGrnList
            .Where(x => x.GateEntryId > 0)
            .Select(x => x.GateEntryId)
            .Distinct()
            .ToList();

        var gateInwardLookupResult = gateEntryIds.Any()
            ? await _gateInwardLookup.GetByIdsAsync(gateEntryIds, cancellationToken)
            : Array.Empty<Contracts.Dtos.Lookups.Gate.GateInwardLookupDto>() as IReadOnlyList<Contracts.Dtos.Lookups.Gate.GateInwardLookupDto>;

        var gateInwardResults = gateInwardLookupResult.ToDictionary(g => g.Id, g => g);

        // Enrich DTOs
        foreach (var po in pendingGrnList)
        {
            // 🧾 Party name
            if (po.PartyId > 0 && partyResults.TryGetValue(po.PartyId, out var party))
                po.PartyName = party.PartyName;
            else
                po.PartyName = "NA";

            // 🏭 Receiving Warehouse
            if (po.ReceivingWarehouseId.HasValue &&
                warehouseResults.TryGetValue(po.ReceivingWarehouseId.Value, out var receivingWarehouse))
                po.ReceivingWarehouseName = receivingWarehouse.WarehouseName;
            else
                po.ReceivingWarehouseName = "NA";

            // 🧪 QC Warehouse
            if (po.QcWarehouseId.HasValue &&
                warehouseResults.TryGetValue(po.QcWarehouseId.Value, out var qcWarehouse))
                po.QcWarehouseName = qcWarehouse.WarehouseName;
            else
                po.QcWarehouseName = "NA";

            // 🚚 Gate Inward (centralized) — null when GateEntryId points at deprecated
            // Purchase.GateEntryHeader rows or when the Gate row was soft-deleted.
            if (po.GateEntryId > 0 && gateInwardResults.TryGetValue(po.GateEntryId, out var gateInward))
            {
                po.GateEntryNo = gateInward.GateEntryNo;
                po.GateEntryDate = gateInward.GateEntryDate;
            }
        }
             //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetGrnPendingHeaderQueryHandler",        
                    actionName: pendingGrnList.Count.ToString(),
                    details: $"Pending PO details was fetched.",
                    module:"GRNEntry"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
              return new ApiResponseDTO<List<GetGrnPendingHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = pendingGrnList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
