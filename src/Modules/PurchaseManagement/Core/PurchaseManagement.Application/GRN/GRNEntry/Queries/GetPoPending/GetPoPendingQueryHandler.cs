using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetPoPending
{
    public class GetPoPendingQueryHandler : IRequestHandler<GetPoPendingQuery, List<GetPoPendingDto>>
    {
        private readonly IGRNEntryQueryRepository _grnEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IPartyLookup _partyLookup;
        private readonly IUnitLookup _unitLookup;

        public GetPoPendingQueryHandler(
            IGRNEntryQueryRepository grnEntryQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IPartyLookup partyLookup,
            IUnitLookup unitLookup)
        {
            _grnEntryQueryRepository = grnEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _partyLookup = partyLookup;
            _unitLookup = unitLookup;
        }

        public async Task<List<GetPoPendingDto>> Handle(GetPoPendingQuery request, CancellationToken cancellationToken)
        {
            var result = await _grnEntryQueryRepository.GetPoPendingAsync();
            var pendingPoList = _mapper.Map<List<GetPoPendingDto>>(result);

            // 1. Collect unique VendorIds and UnitIds
            var vendorIds = pendingPoList
                .Where(po => po.VendorId > 0)
                .Select(po => po.VendorId)
                .Distinct()
                .ToList();

            var unitIds = pendingPoList
                .Where(po => po.UnitId > 0)
                .Select(po => po.UnitId)
                .Distinct()
                .ToList();

            // 2. Fire lookup calls in parallel
            var vendorTask = _partyLookup.GetByIdsAsync(vendorIds, cancellationToken);
            var unitTask = _unitLookup.GetAllUnitAsync();

            // 3. Await all
            await Task.WhenAll(vendorTask, unitTask);

            // 4. Build lookup dictionaries
            var vendorLookup = vendorTask.Result
                .Where(v => v != null)
                .ToDictionary(v => v.Id, v => v);

            var unitLookup = unitTask.Result
                .Where(u => u != null)
                .ToDictionary(u => u.UnitId, u => u.UnitName);

            // 5. Enrich DTOs
            foreach (var po in pendingPoList)
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

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetPoPendingQuery",
                actionName: pendingPoList.Count.ToString(),
                details: "Pending PO details was fetched.",
                module: "GRNEntry"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return pendingPoList;
        }
    }
}
