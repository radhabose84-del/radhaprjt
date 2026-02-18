#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetGroup;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetGroup.Queries.GetAssetGroupAutoComplete
{
    public class GetAssetGroupAutoCompleteQueryHandler: IRequestHandler<GetAssetGroupAutoCompleteQuery,List<AssetGroupAutoCompleteDTO>>
    {
        private readonly IAssetGroupQueryRepository _iAssetGroupQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetGroupAutoCompleteQueryHandler(IAssetGroupQueryRepository iAssetGroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetGroupQueryRepository = iAssetGroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<AssetGroupAutoCompleteDTO>> Handle(GetAssetGroupAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetGroupQueryRepository.GetAssetGroups(request.SearchPattern);
            var assetGroups = _mapper.Map<List<AssetGroupAutoCompleteDTO>>(result);
             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "",
                    details: $"AssetGroup details was fetched.",
                    module:"AssetGroup"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return assetGroups;
        }
    }
}