#nullable disable
using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAmc.Queries.GetExistingVendorDetails
{
    public class GetExistingVendorDetailsQueryHandler :  IRequestHandler<GetExistingVendorDetailsQuery,List<GetExistingVendorDetailsDto>>
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IAssetAmcQueryRepository _iAssetAmcQueryRepository;
        public GetExistingVendorDetailsQueryHandler(IMapper mapper, IMediator mediator, IAssetAmcQueryRepository iAssetAmcQueryRepository)
        {
            _mapper = mapper;
            _mediator = mediator;
            _iAssetAmcQueryRepository = iAssetAmcQueryRepository;   
        }

        public async Task<List<GetExistingVendorDetailsDto>> Handle(GetExistingVendorDetailsQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetAmcQueryRepository.GetVendorDetails(request.OldUnitCode,request.VendorCode);
            var assetunits  = _mapper.Map<List<GetExistingVendorDetailsDto>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "GetAllVendorDetails",        
                    actionName: request.OldUnitCode,
                    details: $"Vendor details was fetched.",
                    module:"ExistingVendorDetails"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return assetunits;
        }
    }
}