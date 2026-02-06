using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using UserManagement.Domain.Enums.Common;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.UserGroup.Commands.DeleteUserGroup
{
    public class DeleteUserGroupCommandHandler : IRequestHandler<DeleteUserGroupCommand, UserGroupDto>
    {
        private readonly IUserGroupCommandRepository _userGroupRepository;
        private readonly IUserGroupQueryRepository _userGroupQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        
        public DeleteUserGroupCommandHandler(IUserGroupCommandRepository userGroupRepository, IMapper mapper, IUserGroupQueryRepository userGroupQueryRepository, IMediator mediator)
        {
            _userGroupRepository = userGroupRepository;
            _mapper = mapper;
            _userGroupQueryRepository = userGroupQueryRepository;
            _mediator = mediator;
        }       
        public async Task<UserGroupDto> Handle(DeleteUserGroupCommand request, CancellationToken cancellationToken)
        {
            var userGroup = await _userGroupQueryRepository.GetByIdAsync(request.Id);
            if (userGroup is null || userGroup.IsDeleted is Enums.IsDelete.Deleted)
            {
                throw new ValidationException("Invalid GroupID. The specified GroupName does not exist or is inactive.");
             
            }         
          
            var userGroupDelete = _mapper.Map<UserManagement.Domain.Entities.UserGroup>(request);
            var updateResult = await _userGroupRepository.DeleteAsync(request.Id, userGroupDelete);
            if (updateResult > 0)
            {
                var userGroupDto = _mapper.Map<UserGroupDto>(userGroupDelete); 
                //Domain Event  
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: userGroupDto.GroupCode ?? string.Empty,
                    actionName: userGroupDto.GroupName ?? string.Empty,
                    details: $"UserGroup '{userGroupDto.GroupName}' was created. GroupCode: {userGroupDto.GroupCode}",
                    module:"UserGroup"
                );               
                await _mediator.Publish(domainEvent, cancellationToken);              
                return userGroupDto;
            }
            throw new Exception("UserGroup deletion failed.");
                 
        }
    }
}