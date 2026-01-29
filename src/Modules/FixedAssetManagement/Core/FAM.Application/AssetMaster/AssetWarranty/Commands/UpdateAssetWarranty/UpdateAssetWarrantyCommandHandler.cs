using AutoMapper;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Commands.UpdateAssetWarranty
{
    public class UpdateAssetWarrantyCommandHandler : IRequestHandler<UpdateAssetWarrantyCommand, bool>
    {
        private readonly IAssetWarrantyCommandRepository _assetWarrantyRepository;
        private readonly IAssetWarrantyQueryRepository _assetWarrantyQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public UpdateAssetWarrantyCommandHandler(IAssetWarrantyCommandRepository assetWarrantyRepository, IMapper mapper,IAssetWarrantyQueryRepository assetWarrantyQueryRepository, IMediator mediator)
        {
            _assetWarrantyRepository = assetWarrantyRepository;
            _mapper = mapper;
            _assetWarrantyQueryRepository = assetWarrantyQueryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(UpdateAssetWarrantyCommand request, CancellationToken cancellationToken)
        {
            var assetWarranty = await _assetWarrantyQueryRepository.GetByIdAsync(request.Id);
            if (assetWarranty is null)
            throw new ValidationException("Invalid DepreciationGroupID. The specified Name does not exist");
           
           
            var oldAssetWarranty= assetWarranty.Id;            
        
           
            var updatedAssetSpecEntity = _mapper.Map<AssetWarranties>(request);                   
            var updateResult = await _assetWarrantyRepository.UpdateAsync(updatedAssetSpecEntity);            

                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Update",
                    actionCode: request.AssetId.ToString(),
                    actionName: request.WarrantyType.ToString() ?? string.Empty,
                    details: $"AssetWarranty '{oldAssetWarranty}' was updated to '{request.Description}'",
                    module:"AssetWarranty"
                );            
                await _mediator.Publish(domainEvent, cancellationToken);
                if(updateResult)
                {
                    
                    return updateResult;
                }
                throw new Exception("AssetWarranty not updated.");
                            
            }           
    }
}