using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByDeptId
{
    public class GetCategoryByDeptIQueryHandler  : IRequestHandler<GetCategoryByDeptIQuery,  List<GetCategoryByDeptIdDto>>
    {
        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;

           private readonly IMapper _mapper;        
        private readonly IMediator _mediator; 

           public GetCategoryByDeptIQueryHandler( IAssetTransferQueryRepository assetTransferQueryRepository, IMapper mapper, IMediator mediator)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
         
        }

          public async Task<List<GetCategoryByDeptIdDto>> Handle(GetCategoryByDeptIQuery request, CancellationToken cancellationToken)
        {
            var CategoryList = await _assetTransferQueryRepository.GetCategoriesByDepartmentAsync(request.DepartmentId);

             
            var AssetTransferList = _mapper.Map<List<GetCategoryByDeptIdDto>>(CategoryList);

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