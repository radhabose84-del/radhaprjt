
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.UserGroup.Commands.CreateUserGroup
{
    public class CreateUserGroupCommandHandler  : IRequestHandler<CreateUserGroupCommand, UserGroupDto>
{
    private readonly IMapper _mapper;
    private readonly IUserGroupCommandRepository _userGroupRepository;    
    private readonly IMediator _mediator; 

    // Constructor Injection
    public CreateUserGroupCommandHandler(IMapper mapper, IUserGroupCommandRepository userGroupRepository, IMediator mediator)
    {
        _mapper = mapper;
        _userGroupRepository = userGroupRepository; 
        _mediator = mediator;               
    }

    public async Task<UserGroupDto> Handle(CreateUserGroupCommand request, CancellationToken cancellationToken)
    {
        var userGroupExists = await _userGroupRepository.GetUserGroupByCodeAsync(request.GroupName ?? string.Empty,request.GroupCode ?? string.Empty);        
        if (userGroupExists.Id !=0)
        {
            throw new ValidationException("GroupCode already exists");
          
        }
        var userGroupEntity = _mapper.Map<UserManagement.Domain.Entities.UserGroup>(request);    
         
        var result = await _userGroupRepository.CreateAsync(userGroupEntity);
        if (result != null)
        {
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: result?.GroupCode ?? string.Empty,
                actionName: result?.GroupName ?? string.Empty,
                details: $"USerGroup '{result?.GroupName}' was created. GroupCode: {result?.GroupCode}",
                module:"USerGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            
            var userGroupDto = _mapper.Map<UserGroupDto>(result);
            if (userGroupDto.Id > 0)
            {
                return userGroupDto;
            }
        }
        throw new Exception("UserGroup not created");
       
    }
    }
}
