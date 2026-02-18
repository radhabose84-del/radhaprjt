using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;

namespace PurchaseManagement.Application.GRN.GateEntry.Queries.GetGateEntriesApprovedPo
{
    public class GetGateEntriesApprovedPoQueryHandler : IRequestHandler<GetGateEntriesApprovedPoQuery, List<GetGateEntriesApprovedPoDto>>
    {
        private readonly IGateEntryQueryRepository _iGateEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IPartyLookup _partyLookup;
        private readonly IUnitLookup _unitLookup;

        public GetGateEntriesApprovedPoQueryHandler(IGateEntryQueryRepository iGateEntryQueryRepository, IMapper mapper, IMediator mediator
        , IPartyLookup partyLookup, IUnitLookup unitGrpcLookup
        )
        {
            _iGateEntryQueryRepository = iGateEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _partyLookup = partyLookup;
            _unitLookup = unitGrpcLookup;
        }

        public async Task<List<GetGateEntriesApprovedPoDto>> Handle(GetGateEntriesApprovedPoQuery request, CancellationToken cancellationToken)
        {
             var result = await _iGateEntryQueryRepository.GetGateEntriesApprovedPoDto(request.PartyId);
            var pendingpoIds = _mapper.Map<List<GetGateEntriesApprovedPoDto>>(result);
             //1️⃣ Collect unique VendorIds and UnitIds
            var vendorIds = pendingpoIds
                .Where(po => po.VendorId > 0)
                .Select(po => po.VendorId)
                .Distinct()
                .ToList();

            var unitIds = pendingpoIds
                .Where(po => po.UnitId > 0)
                .Select(po => po.UnitId)
                .Distinct()
                .ToList();

            //2️⃣ Fire gRPC calls in parallel
            var vendorTask = _partyLookup.GetByIdsAsync(vendorIds, cancellationToken);
            var unitTask = _unitLookup.GetAllUnitAsync();

            //3️⃣ Await all
            await Task.WhenAll(vendorTask, unitTask);

           // 4️⃣ Build lookup dictionaries
            var vendorLookup = vendorTask.Result
                .Where(v => v != null)
                .ToDictionary(v => v.Id, v => v);

            var unitLookup = unitTask.Result
                .Where(u => u != null)
                .ToDictionary(u => u.UnitId, u => u.UnitName);

            //5️⃣ Enrich DTOs
            foreach (var po in pendingpoIds)
            {
                // Vendor details
                if (po.VendorId > 0 && vendorLookup.TryGetValue(po.VendorId, out var vendor))
                {
                    po.VendorName = vendor.PartyName;
                    po.GSTNumber = po.GSTNumber ?? "NA";
                    po.ContactName = vendor.Mobile ?? vendor.Email ?? string.Empty;
                }
                else
                {
                    po.VendorName = "NA";
                    po.GSTNumber = "NA";
                    po.ContactName = string.Empty;
                }

                // Unit name
                if (po.UnitId > 0 && unitLookup.TryGetValue(po.UnitId, out var unitName))
                {
                    po.UnitName = unitName;
                }
                else
                {
                    po.UnitName = "NA";
                }
            }
            
            
             //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetGateEntriesApprovedPoQuery",        
                    actionName: pendingpoIds.Count.ToString(),
                    details: $"Pending PO details was fetched.",
                    module:"GateEntry"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return pendingpoIds;
        }
    }
}
