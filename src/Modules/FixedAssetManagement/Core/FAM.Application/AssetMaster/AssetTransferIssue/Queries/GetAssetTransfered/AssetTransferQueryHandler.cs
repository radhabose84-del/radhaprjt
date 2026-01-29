using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransfer.Queries.GetAssetTransfered
{
    public class AssetTransferQueryHandler : IRequestHandler<AssetTransferQuery,  ApiResponseDTO<List<AssetTransferDto>>>
    {
        private readonly IAssetTransferQueryRepository _assetTransferQueryRepository;
        private readonly IMapper _mapper;        
        private readonly IMediator _mediator; 
        public AssetTransferQueryHandler( IAssetTransferQueryRepository assetTransferQueryRepository, IMapper mapper, IMediator mediator)
        {
            _assetTransferQueryRepository = assetTransferQueryRepository;
             _mapper = mapper;
            _mediator = mediator;
        }
         public  async Task<ApiResponseDTO<List<AssetTransferDto>>> Handle(AssetTransferQuery request, CancellationToken cancellationToken)        
        {
           // var (assetInsurance, totalCount) = await _assetInsuranceQueryRepository.GetAllAssetInsuranceAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var (assetTransferList, totalCount)  = await _assetTransferQueryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm ,request.FromDate, request.ToDate);
          //  var totalCount = assetInsurance.Count;
            var AssetTransferList = _mapper.Map<List<AssetTransferDto>>(assetTransferList);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"Asset Transfer    details was fetched.",
                module:"Asset Insurance"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<AssetTransferDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = AssetTransferList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize                
            };          
        }

      
        
    }
}