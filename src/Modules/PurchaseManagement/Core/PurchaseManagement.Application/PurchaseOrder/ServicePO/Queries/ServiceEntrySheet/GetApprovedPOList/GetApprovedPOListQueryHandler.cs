using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetApprovedPOList
{

    public class GetApprovedPOListQueryHandler : IRequestHandler<GetApprovedPOListQuery, List<PoIdNumberDto>>
    {

        private readonly IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetApprovedPOListQueryHandler(IServicePurchaseOrderQueryRepository servicePurchaseOrderQueryRepository, IMapper mapper, IMediator mediator)
        {
            _servicePurchaseOrderQueryRepository = servicePurchaseOrderQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<PoIdNumberDto>> Handle(GetApprovedPOListQuery request, CancellationToken ct)
        {
            var rows = await _servicePurchaseOrderQueryRepository.GetApprovedServicePoAsync();
            return rows?.ToList() ?? new List<PoIdNumberDto>();
        }

    }
}
