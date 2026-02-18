using AutoMapper;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetSubGroup.Queries.GetAssetSubGroupById
{
    public class GetAssetSubGroupByIdQueryHandler : IRequestHandler<GetAssetSubGroupByIdQuery,AssetSubGroupDto>
    {
        private readonly IAssetSubGroupQueryRepository _iAssetSubGroupQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAssetSubGroupByIdQueryHandler(IAssetSubGroupQueryRepository iAssetSubGroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetSubGroupQueryRepository = iAssetSubGroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<AssetSubGroupDto> Handle(GetAssetSubGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetSubGroupQueryRepository.GetByIdAsync(request.Id);
            // Check if the entity exists
            if (result is null)
            {
                throw new ValidationException( $"AssetSubGroup ID {request.Id} not found.");
                
            }
            // Map a single entity
            var assetSubGroup = _mapper.Map<AssetSubGroupDto>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"AssetSubGroup details {assetSubGroup.Id} was fetched.",
                    module:"AssetSubGroup"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return assetSubGroup;

        }
    }
}