
using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.DepreciationGroup.Queries.GetDepreciationGroupById
{
    public class GetDepreciationGroupByIdQueryHandler : IRequestHandler<GetDepreciationGroupByIdQuery, DepreciationGroupDTO>
    {
        private readonly IDepreciationGroupQueryRepository _depreciationGroupRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetDepreciationGroupByIdQueryHandler(IDepreciationGroupQueryRepository depreciationGroupRepository,  IMapper mapper, IMediator mediator)
        {
            _depreciationGroupRepository =depreciationGroupRepository;
            _mapper =mapper;
            _mediator = mediator;
        }

        public async Task<DepreciationGroupDTO> Handle(GetDepreciationGroupByIdQuery request, CancellationToken cancellationToken)
        {
            {
                var depreciationGroup = await _depreciationGroupRepository.GetByIdAsync(request.Id);   
                if (depreciationGroup is null)
                    throw new EntityNotFoundException("DepreciationGroup", request.Id);

                var depreciationGroupDto = _mapper.Map<DepreciationGroupDTO>(depreciationGroup);
            
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: depreciationGroupDto.Code ?? string.Empty,        
                    actionName: depreciationGroupDto.DepreciationGroupName ?? string.Empty,                
                    details: $"DepreciationGroup '{depreciationGroupDto.DepreciationGroupName}' was created. Code: {depreciationGroupDto.Code}",
                    module:"DepreciationGroup"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                return depreciationGroupDto;
            }
        }
    }
}