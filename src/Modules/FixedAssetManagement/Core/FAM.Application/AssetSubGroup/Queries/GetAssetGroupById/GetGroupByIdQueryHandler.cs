using AutoMapper;
using FAM.Application.AssetGroup.Queries.GetAssetGroupById;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FAM.Application.AssetSubGroup.Queries.GetAssetGroupById
{
    public class GetGroupByIdQueryHandler : IRequestHandler<GetGroupByIdQuery,List<AssetSubGroupDto>>
    {
        private readonly IAssetSubGroupQueryRepository _iAssetSubGroupQueryRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetGroupByIdQueryHandler(IAssetSubGroupQueryRepository iAssetSubGroupQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iAssetSubGroupQueryRepository = iAssetSubGroupQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<AssetSubGroupDto>> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _iAssetSubGroupQueryRepository.GetByGroupIdAsync(request.GroupId);
            // Check if the entity exists
            if (result is null)
            {
                throw new ValidationException($"AssetSubGroup ID {request.GroupId} not found.");
                
            }
            // Map a single entity
            var assetSubGroup = _mapper.Map<List<AssetSubGroupDto>>(result);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "",
                actionName: "",
                details: $"AssetSubGroup details  was fetched.",
                module: "AssetSubGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            
           return  assetSubGroup;

        }
    }
}