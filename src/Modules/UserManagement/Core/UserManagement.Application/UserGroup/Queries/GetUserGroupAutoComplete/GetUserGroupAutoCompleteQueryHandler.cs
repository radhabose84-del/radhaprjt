using AutoMapper;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.UserGroup.Queries.GetUserGroupAutoComplete
{
    public class GetUserGroupAutoCompleteQueryHandler : IRequestHandler<GetUserGroupAutoCompleteQuery, List<UserGroupAutoCompleteDto>>
    {

        private readonly IUserGroupQueryRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;         


		public GetUserGroupAutoCompleteQueryHandler(IUserGroupQueryRepository userRepository, IMapper mapper, IMediator mediator)
        {
           _userRepository =userRepository;
           _mapper =mapper;
           _mediator = mediator;
        }  
   
        public  async Task<List<UserGroupAutoCompleteDto>> Handle(GetUserGroupAutoCompleteQuery request, CancellationToken cancellationToken)
        {
                           
            var result = await _userRepository.GetUserGroups(request.SearchPattern ?? string.Empty);
            if (result is null || result.Count is 0)
            {
                throw new ValidationException("No user group found matching the search pattern.");
             
            }
            // Publish a domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutoComplete",
                actionCode:"",        
                actionName: request.SearchPattern ?? string.Empty,                
                details: $"User '{request.SearchPattern}' was searched",
                module:"User"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            
            var userDto = _mapper.Map<List<UserGroupAutoCompleteDto>>(result);
            return userDto;
        }
    }
}