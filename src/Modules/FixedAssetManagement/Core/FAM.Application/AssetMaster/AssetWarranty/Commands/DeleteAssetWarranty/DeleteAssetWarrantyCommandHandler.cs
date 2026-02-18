using AutoMapper;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteAssetWarranty
{
    public class DeleteAssetWarrantyCommandHandler : IRequestHandler<DeleteAssetWarrantyCommand, AssetWarrantyDTO>
    {
        private readonly IAssetWarrantyCommandRepository _assetWarrantyRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly IAssetWarrantyQueryRepository _assetWarrantyQueryRepository;
        
        public DeleteAssetWarrantyCommandHandler(IAssetWarrantyCommandRepository assetWarrantyRepository, IMapper mapper,  IMediator mediator,IAssetWarrantyQueryRepository assetWarrantyQueryRepository)
        {
            _assetWarrantyRepository = assetWarrantyRepository;
             _mapper = mapper;        
            _mediator = mediator;
            _assetWarrantyQueryRepository=assetWarrantyQueryRepository;
        }

        public async Task<AssetWarrantyDTO> Handle(DeleteAssetWarrantyCommand request, CancellationToken cancellationToken)
        {             
            var assetWarranty = await _assetWarrantyQueryRepository.GetByIdAsync(request.Id);
            if (assetWarranty is null )
            {
                throw new ValidationException("Invalid DepreciationGroupID.");
              
            }
            var assetWarrantyDelete = _mapper.Map<AssetWarranties>(request);      
            var updateResult = await _assetWarrantyRepository.DeleteAsync(request.Id, assetWarrantyDelete);
            if (updateResult > 0)
            {
                var assetWarrantyDto = _mapper.Map<AssetWarrantyDTO>(assetWarrantyDelete);  
                //Domain Event  
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: assetWarrantyDelete.AssetId.ToString() ?? string.Empty,
                    actionName: assetWarrantyDelete.WarrantyType.ToString() ?? string.Empty,
                    details: $"AssetWarranty '{assetWarrantyDto.Description}' was created.",
                    module:"AssetWarranty"
                );               
                await _mediator.Publish(domainEvent, cancellationToken);                 
                return  assetWarrantyDto;
            }
            throw new Exception("Asset Warranty deletion failed.");
                  
        }
    }
}