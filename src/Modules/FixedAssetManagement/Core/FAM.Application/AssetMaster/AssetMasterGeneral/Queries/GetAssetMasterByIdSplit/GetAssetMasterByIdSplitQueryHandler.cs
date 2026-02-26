using AutoMapper;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterByIdSplit
{
    public class GetAssetMasterByIdSplitQueryHandler : IRequestHandler<GetAssetMasterByIdSplitQuery, AssetMasterSplitDto>
    {
        private readonly IAssetMasterGeneralQueryRepository _assetMasterRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;         

        public GetAssetMasterByIdSplitQueryHandler(IAssetMasterGeneralQueryRepository assetMasterRepository,  IMapper mapper, IMediator mediator)
        {
            _assetMasterRepository =assetMasterRepository;
            _mapper =mapper;
            _mediator = mediator;            
        }
        public async Task<AssetMasterSplitDto> Handle(GetAssetMasterByIdSplitQuery request, CancellationToken cancellationToken)
        {
          //  var assetMaster = await _assetMasterRepository.GetByIdAsync(request.Id);
          var (assetResult, locationResult, purchaseDetails,additionalCost) = await _assetMasterRepository.GetAssetMasterSplitByIdAsync(request.Id);
          var asset = _mapper.Map<AssetMasterSplitDto>(assetResult);
            if (assetResult == null)
            {                
                return null!;
            }
            if (assetResult?.AssetName != null)
             {
                 asset.AssetParent = _mapper.Map<AssetParentDTO>(assetResult);
             }

             if (locationResult != null)
             {
                 asset.AssetLocation = _mapper.Map<AssetLocationDTO>(locationResult);
             }
             if (purchaseDetails != null)
             {
             asset.AssetPurchaseDetails = _mapper.Map<List<AssetPurchaseDetailDTO>>(purchaseDetails);
             }          
            if (additionalCost != null)
             {
                 asset.AssetAdditionalCost = _mapper.Map<List<AssetAdditionalCostDto>>(additionalCost);
             }        

            if (asset is null)
            {               

                throw new ValidationException("AssetName with ID {request.Id} not found."); 
               
            }       
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode:"",        
                actionName: "",                
                details: $"Asset ",
                module:"AssetMasterGeneral"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return asset;       
        }      
    }
}