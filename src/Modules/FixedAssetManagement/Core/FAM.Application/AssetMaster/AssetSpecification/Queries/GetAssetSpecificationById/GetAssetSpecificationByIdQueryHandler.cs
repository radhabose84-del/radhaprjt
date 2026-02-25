#nullable disable
using AutoMapper;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationById
{
    public class GetAssetSpecificationByIdQueryHandler  : IRequestHandler<GetAssetSpecificationByIdQuery, AssetSpecificationJsonDto>
    {
        private readonly IAssetSpecificationQueryRepository _assetSpecificationRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetAssetSpecificationByIdQueryHandler(IAssetSpecificationQueryRepository assetSpecificationRepository,  IMapper mapper, IMediator mediator)
        {
            _assetSpecificationRepository =assetSpecificationRepository;
            _mapper =mapper;
            _mediator = mediator;
        }

        public async Task<AssetSpecificationJsonDto> Handle(GetAssetSpecificationByIdQuery request, CancellationToken cancellationToken)
        {
            var assetSpecification = await _assetSpecificationRepository.GetByIdAsync(request.Id);                
            var assetSpecificationDto = _mapper.Map<AssetSpecificationJsonDto>(assetSpecification);
            if (assetSpecification is null)
            {       
                throw new ValidationException("AssetSpecification with ID {request.Id} not found.");         
                
            }       
                //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: assetSpecificationDto.AssetId.ToString(),
                actionName: assetSpecificationDto.AssetCode,
                details: $"SpecificationMaster '{assetSpecificationDto.AssetName}' was created",
                module:"SpecificationMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return  assetSpecificationDto;       
        }
    }
}