using AutoMapper;
// using Contracts.Interfaces.External.IParty;
// using Contracts.Interfaces.External.IUser;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetVendorServicePO
{
    public class GetVendorServicePOQueryhandler : IRequestHandler<GetVendorServicePOQuery, List<GetVendorServicePODto>>
    {
        private readonly IServiceQueryRepository _iServiceQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        
       
        public GetVendorServicePOQueryhandler(IServiceQueryRepository iServiceQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iServiceQueryRepository = iServiceQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
          
            
        }

        public async Task<List<GetVendorServicePODto>> Handle(GetVendorServicePOQuery request, CancellationToken cancellationToken)
        {
            var result = await _iServiceQueryRepository.GetVendorApprovedServicePo(request.VendorId);

            var pendingpoIds = _mapper.Map<List<GetVendorServicePODto>>(result);

             //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetVendorServicePOQuery",        
                    actionName: pendingpoIds.Count.ToString(),
                    details: $"Pending PO details was fetched.",
                    module:"ServicePO"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return pendingpoIds;
        }
    }
}