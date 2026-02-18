using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssertByCategory
{
    public class GetAssetsByCategoryQueryHandler  : IRequestHandler<GetAssetsByCategoryQuery,  List<GetAssetMasterDto>>
    {


        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;
        private readonly IMapper _mapper;
        public readonly IMediator _mediator;
        public GetAssetsByCategoryQueryHandler( IAssetTransferQueryRepository assetTransferQueryRepository, IMapper mapper, IMediator mediator )
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;
            _mapper = mapper;
            _mediator=mediator;
        }
        public async Task<List<GetAssetMasterDto>> Handle(GetAssetsByCategoryQuery request, CancellationToken cancellationToken)
        {
            var assets = await _assetTransferQueryRepository.GetAssetsByCategoryAsync(request.AssetCategoryId , request.AssetDepartmentId);

            //return _mapper.Map<List<GetAssetMasterDto>>(assets);
            var AssetList = _mapper.Map<List<GetAssetMasterDto>>(assets);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"Asset Category    details was fetched.",
                module:"Asset Category"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return  AssetList;           

          
        }


        
    }
}