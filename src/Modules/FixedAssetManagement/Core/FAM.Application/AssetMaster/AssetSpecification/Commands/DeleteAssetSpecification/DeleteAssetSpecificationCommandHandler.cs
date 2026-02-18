using AutoMapper;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Commands.DeleteAssetSpecification
{
    public class DeleteAssetSpecificationCommandHandler : IRequestHandler<DeleteAssetSpecificationCommand, AssetSpecificationDTO>
    {
        private readonly IAssetSpecificationCommandRepository _assetSpecificationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly IAssetSpecificationQueryRepository _assetSpecificationQueryRepository;
        
        public DeleteAssetSpecificationCommandHandler(IAssetSpecificationCommandRepository assetSpecificationRepository, IMapper mapper,  IMediator mediator,IAssetSpecificationQueryRepository assetSpecificationQueryRepository)
        {
            _assetSpecificationRepository = assetSpecificationRepository;
             _mapper = mapper;        
            _mediator = mediator;
            _assetSpecificationQueryRepository=assetSpecificationQueryRepository;
        }

        public async Task<AssetSpecificationDTO> Handle(DeleteAssetSpecificationCommand request, CancellationToken cancellationToken)
        {             
            var assetSpecifications = await _assetSpecificationQueryRepository.GetByIdAsync(request.Id);
            if (assetSpecifications is null )
            {
                throw new ValidationException("Invalid DepreciationGroupID.");
               
            }
            var assetSpecificationDelete = _mapper.Map<AssetSpecifications>(request);      
            var updateResult = await _assetSpecificationRepository.DeleteAsync(request.Id, assetSpecificationDelete);
            if (updateResult > 0)
            {
                var assetSpecificationDto = _mapper.Map<AssetSpecificationDTO>(assetSpecificationDelete);  
                //Domain Event  
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: assetSpecificationDelete.AssetId.ToString() ?? string.Empty,
                    actionName: assetSpecificationDelete.SpecificationId.ToString() ?? string.Empty,
                    details: $"AssetSpecification '{assetSpecificationDto.SpecificationValue}' was created.",
                    module:"AssetSpecification"
                );               
                await _mediator.Publish(domainEvent, cancellationToken);                 
                return assetSpecificationDto;
            }
            throw new Exception("Asset Specification deletion failed.");
                   
        }
    }
}