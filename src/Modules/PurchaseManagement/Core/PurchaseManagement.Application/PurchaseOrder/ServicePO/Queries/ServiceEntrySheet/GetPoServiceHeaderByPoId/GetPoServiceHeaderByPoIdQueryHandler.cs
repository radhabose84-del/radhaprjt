using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetPoServiceHeaderByPoId
{
    public class GetPoServiceHeaderByPoIdQueryHandler : IRequestHandler<GetPoServiceHeaderByPoIdQuery, PoServiceHeaderByIdDto?>
    {
       
          private readonly  IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
          private readonly IMapper _mapper;
          private readonly IMediator _mediator;
          private readonly IUnitLookup _unitLookup;
          private readonly IPartyLookup _partyLookup;

        public GetPoServiceHeaderByPoIdQueryHandler(IServicePurchaseOrderQueryRepository servicePurchaseOrderQueryRepository, IMapper mapper, IMediator mediator, IUnitLookup unitLookup, IPartyLookup partyLookup)
        {
            _servicePurchaseOrderQueryRepository = servicePurchaseOrderQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _unitLookup = unitLookup;
            _partyLookup = partyLookup;

        }
        
        public async Task<PoServiceHeaderByIdDto?> Handle(GetPoServiceHeaderByPoIdQuery request, CancellationToken cancellationToken)
        {

              var units = await _unitLookup.GetAllUnitAsync();
              var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);      
          

            // Fetch (repo may return multiple service headers for a single PO)
            var entities = await _servicePurchaseOrderQueryRepository
                .GetServicePoHeaderByIdAsync(request.PoId, cancellationToken);

                if (entities is null)
            return null;

            // Map to DTOs (handle null from repo)
            var headers = _mapper.Map<PoServiceHeaderByIdDto>(entities );

           
                if (headers.VendorId > 0)
                {
                    var partyDetails = await _partyLookup.GetByIdsAsync(new[] { headers.VendorId }, cancellationToken);
                    var party = partyDetails.FirstOrDefault();
                    if (party != null)
                    {
                        headers.VendorName = party.PartyName;
                        headers.VendorCode = party.PartyCode;
                    }
                }
            

              if (unitLookup.TryGetValue(headers.UnitId, out var unitName))
                headers.UnitName = unitName;

            // Domain Event (log how many were fetched)
            var count = 1;
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetPoServiceHeaderByPoIdQuery",
                actionName: $"Fetched {count} header(s)",
                details: $"Service PO header(s) fetched for PoId={request.PoId}.",
                module: "SES PO"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // You currently return only one item; keep that behavior:
            return headers;
        }

     
    }
}
