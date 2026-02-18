using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using UserManagement.Domain.Enums.Common;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.UserGroup.Commands.UpdateUesrGroup
{
    public class UpdateUserGroupCommandHandler  : IRequestHandler<UpdateUserGroupCommand,bool>
    {
        private readonly IUserGroupCommandRepository _userGroupRepository;
        private readonly IMapper _mapper;
        private readonly IUserGroupQueryRepository _userGroupQueryRepository;
        private readonly IMediator _mediator; 

        public UpdateUserGroupCommandHandler(IUserGroupCommandRepository userGroupRepository, IMapper mapper, IUserGroupQueryRepository userGroupQueryRepository, IMediator mediator)
        {
            _userGroupRepository = userGroupRepository;
             _mapper = mapper;
            _userGroupQueryRepository = userGroupQueryRepository;
            _mediator = mediator;
        }       
        public async Task<bool> Handle(UpdateUserGroupCommand request, CancellationToken cancellationToken)
        {
            var userGroup = await _userGroupQueryRepository.GetByIdAsync(request.Id);
            if (userGroup is null)
            throw new ValidationException("UserGroup not found");
              
            var oldGroupName = userGroup.GroupName;
            userGroup.GroupName = request.GroupName;
            if (userGroup is null || userGroup.IsDeleted is Enums.IsDelete.Deleted)
            {
                throw new ValidationException("Invalid UserGroupID. The specified UserGroup does not exist or is inactive.");
           
            }   
                      
            if ((byte)userGroup.IsActive != request.IsActive)
            {    
                 userGroup.IsActive =  (Enums.Status)request.IsActive;             
                await _userGroupRepository.UpdateAsync(userGroup.Id, userGroup);
                if (request.IsActive is 0)
                {
                    return true;
                }
                else{
                    return true; 
                }                                     
            }
        
            var updatedUserGroupEntity = _mapper.Map<UserManagement.Domain.Entities.UserGroup>(request);
            
            var updateResult = await _userGroupRepository.UpdateAsync(request.Id, updatedUserGroupEntity);            
            var updatedUserGroups = await _userGroupQueryRepository.GetByIdAsync(request.Id);
            
            if (updatedUserGroups != null)
            {
                var userGroupDto = _mapper.Map<UserGroupDto>(updatedUserGroups);
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Update",
                    actionCode: userGroupDto.GroupCode ?? string.Empty,
                    actionName: userGroupDto.GroupName ?? string.Empty,                            
                    details: $"UserGroup '{oldGroupName}' was updated to '{userGroupDto.GroupName}'.  UserGroupCode: {userGroupDto.GroupCode}",
                    module:"UserGroup"
                );            
                await _mediator.Publish(domainEvent, cancellationToken);
                if(updateResult>0)
                {
                    return true;
                }
                throw new Exception("UserGroup not updated.");
              
            }
            else
            {
                throw new Exception("UserGroup update failed");
               
            }                   
        }
    }
}