using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByCustodian
{
    public class GetCategoryByCustodianQueryHandler : IRequestHandler<GetCategoryByCustodianQuery, List<GetCategoryByCustodianDto>>
    {
        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;

        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCategoryByCustodianQueryHandler(IAssetTransferQueryRepository assetTransferQueryRepository, IMapper mapper, IMediator mediator)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
          public async Task<List<GetCategoryByCustodianDto>> Handle(GetCategoryByCustodianQuery request, CancellationToken cancellationToken)
        {
            var CategoryList = await _assetTransferQueryRepository.GetCategoryByCustodianAsync(request.CustodianId ?? string.Empty,request.DepartmentId);

             
            var AssetTransferList = _mapper.Map<List<GetCategoryByCustodianDto>>(CategoryList);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"Asset Category    details was fetched.",
                module:"Asset Category"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return AssetTransferList;                 
        }
    }
}