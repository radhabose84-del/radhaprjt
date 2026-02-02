using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoMapper;
// using Contracts.Interfaces.External.IParty;
// using Contracts.Interfaces.External.IUser;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.GRN.GateEntry.Queries.GetGateEntriesApprovedPo
{
    public class GetGateEntriesApprovedPoQueryHandler : IRequestHandler<GetGateEntriesApprovedPoQuery, List<GetGateEntriesApprovedPoDto>>
    {
        private readonly IGateEntryQueryRepository _iGateEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        // private readonly IPartyGrpcClient _partyGrpcClient;
        // private readonly IUnitGrpcClient _unitGrpcClient;

        public GetGateEntriesApprovedPoQueryHandler(IGateEntryQueryRepository iGateEntryQueryRepository, IMapper mapper, IMediator mediator
        // , IPartyGrpcClient partyGrpcClient, IUnitGrpcClient unitGrpcClient
        )
        {
            _iGateEntryQueryRepository = iGateEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            // _partyGrpcClient = partyGrpcClient;
            // _unitGrpcClient = unitGrpcClient;
        }

        public async Task<List<GetGateEntriesApprovedPoDto>> Handle(GetGateEntriesApprovedPoQuery request, CancellationToken cancellationToken)
        {
             var result = await _iGateEntryQueryRepository.GetGateEntriesApprovedPoDto(request.PartyId);
            var pendingpoIds = _mapper.Map<List<GetGateEntriesApprovedPoDto>>(result);
             // 1️⃣ Collect unique VendorIds and UnitIds
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

            // 2️⃣ Fire gRPC calls in parallel
            // var vendorTasks = vendorIds.Select(id => _partyGrpcClient.GetPartyByIdAsync(id)).ToList();
            // var unitTask = _unitGrpcClient.GetAllUnitAsync();

            // 3️⃣ Await all
            // await Task.WhenAll(Task.WhenAll(vendorTasks), unitTask);

            // 4️⃣ Build lookup dictionaries
            // var vendorLookup = vendorTasks
            //     .Where(t => t.Result != null)
            //     .Select(t => t.Result)
            //     .ToDictionary(v => v.Id, v => v);

            // var unitLookup = unitTask.Result
            //     .Where(u => u != null)
            //     .ToDictionary(u => u.UnitId, u => u.UnitName);

            // 5️⃣ Enrich DTOs
            // foreach (var po in pendingpoIds)
            // {
            //     // Vendor details
            //     if (po.VendorId > 0 && vendorLookup.TryGetValue(po.VendorId, out var vendor))
            //     {
            //         po.VendorName = vendor.PartyName;
            //         po.GSTNumber = vendor.PartyGst;

            //         var contact = vendor.Contacts?.FirstOrDefault();
            //         po.ContactName = contact?.FirstName ?? string.Empty;
            //     }
            //     else
            //     {
            //         po.VendorName = "NA";
            //         po.GSTNumber = "NA";
            //         po.ContactName = string.Empty;
            //     }

            //     // Unit name
            //     if (po.UnitId > 0 && unitLookup.TryGetValue(po.UnitId, out var unitName))
            //     {
            //         po.UnitName = unitName;
            //     }
            //     else
            //     {
            //         po.UnitName = "NA";
            //     }
            // }
            
            
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