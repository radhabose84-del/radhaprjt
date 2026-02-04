using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.UserGroup.Queries.GetUserGroup
{
    public class GetUserGroupQueryHandler : IRequestHandler<GetUserGroupQuery, ApiResponseDTO<List<UserGroupDto>>>
    
    {
        private readonly IUserGroupQueryRepository _userGroupRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        public GetUserGroupQueryHandler(IUserGroupQueryRepository userGroupRepository , IMapper mapper, IMediator mediator)
        {
            _userGroupRepository = userGroupRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<List<UserGroupDto>>> Handle(GetUserGroupQuery request, CancellationToken cancellationToken)
        {            
            var (userGroups, totalCount)= await _userGroupRepository.GetAllUserGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var userGroupsList = _mapper.Map<List<UserGroupDto>>(userGroups);
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",        
                actionName: "",
                details: $"UserGroup details was fetched.",
                module:"UserGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<UserGroupDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = userGroupsList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
           
        }
    }
}