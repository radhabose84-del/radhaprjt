using AutoMapper;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetWarranty.Commands.CreateAssetWarranty
{
    public class CreateAssetWarrantyCommandHandler : IRequestHandler<CreateAssetWarrantyCommand, AssetWarrantyDTO>
    {
        private readonly IMapper _mapper;
        private readonly IAssetWarrantyCommandRepository _assetWarrantyRepository;
        private readonly IMediator _mediator;

        public CreateAssetWarrantyCommandHandler(IMapper mapper, IAssetWarrantyCommandRepository assetWarrantyRepository, IMediator mediator)
        {
            _mapper = mapper;
            _assetWarrantyRepository = assetWarrantyRepository;
            _mediator = mediator;    
        } 

        public async Task<AssetWarrantyDTO> Handle(CreateAssetWarrantyCommand request, CancellationToken cancellationToken)
        {
            var assetSpecificationExists = await _assetWarrantyRepository.ExistsByAssetIdAsync(request.AssetId);
            if (assetSpecificationExists)
            {
                throw new ValidationException("Asset Warranty already exists.");
                              
            }
            var assetEntity = _mapper.Map<AssetWarranties>(request);     
            var result = await _assetWarrantyRepository.CreateAsync(assetEntity);
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: assetEntity.AssetId.ToString() ?? string.Empty,
                actionName: assetEntity.WarrantyType.ToString() ?? string.Empty,
                details: $"Asset Warranty '{assetEntity.Description}' was created.",
                module:"Asset Warranty"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            
            var assetMasterDTO = _mapper.Map<AssetWarrantyDTO>(result);
            if (assetMasterDTO.Id > 0)
            {
                return  assetMasterDTO;
            }
            throw new Exception("Asset Warranty not created.");
                
        }
    }
}