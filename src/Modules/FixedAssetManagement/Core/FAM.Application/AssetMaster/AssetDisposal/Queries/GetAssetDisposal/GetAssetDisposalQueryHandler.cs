using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetDisposal;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposal
{
    public class GetAssetDisposalQueryHandler : IRequestHandler<GetAssetDisposalQuery,ApiResponseDTO<List<AssetDisposalDto>>>
    {
        private readonly IAssetDisposalQueryRepository _iAssetDisposalQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetDisposalQueryHandler(IAssetDisposalQueryRepository iAssetDisposalQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetDisposalQueryRepository = iAssetDisposalQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AssetDisposalDto>>> Handle(GetAssetDisposalQuery request, CancellationToken cancellationToken)
        {
             var (assetdisposal, totalCount) = await _iAssetDisposalQueryRepository.GetAllAssetDisposalAsync(request.PageNumber, request.PageSize, request.SearchTerm);
             var assetdisposalgruplist = _mapper.Map<List<AssetDisposalDto>>(assetdisposal);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAssetDisposalQuery",
                    actionCode: "GetAll",        
                    actionName: assetdisposalgruplist.Count.ToString(),
                    details: $"AssetDisposal details was fetched.",
                    module:"AssetDisposal"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetDisposalDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = assetdisposalgruplist ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };
        }
        

    }
}