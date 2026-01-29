using AutoMapper;
using FAM.Application.Common.HttpResponse;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Domain.Events;
using MediatR;

namespace FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup
{
    public class GetDepreciationGroupQueryHandler : IRequestHandler<GetDepreciationGroupQuery, (List<DepreciationGroupDTO>, int)>
    {
        private readonly IDepreciationGroupQueryRepository _depreciationGroupRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        public GetDepreciationGroupQueryHandler(IDepreciationGroupQueryRepository depreciationGroupRepository , IMapper mapper, IMediator mediator)
        {
            _depreciationGroupRepository = depreciationGroupRepository;
            _mapper = mapper;
            _mediator = mediator;
        }        
        public async Task<(List<DepreciationGroupDTO>, int)> Handle(GetDepreciationGroupQuery request, CancellationToken cancellationToken)
        {
            var (depreciationGroup, totalCount) = await _depreciationGroupRepository.GetAllDepreciationGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var depreciationGroupList = _mapper.Map<List<DepreciationGroupDTO>>(depreciationGroup);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"DepreciationGroup details was fetched.",
                module:"DepreciationGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
           return (depreciationGroupList, totalCount);
        }
    }
}