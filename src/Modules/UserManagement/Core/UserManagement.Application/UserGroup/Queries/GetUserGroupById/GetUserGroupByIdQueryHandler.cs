using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroup;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.UserGroup.Queries.GetUserGroupById
{
    public class GetUserGroupByIdQueryHandler  : IRequestHandler<GetUserGroupByIdQuery, UserGroupDto>
    {
        private readonly IUserGroupQueryRepository _userGroupRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetUserGroupByIdQueryHandler(IUserGroupQueryRepository userGroupRepository, IMapper mapper, IMediator mediator)
        {
            _userGroupRepository = userGroupRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<UserGroupDto> Handle(GetUserGroupByIdQuery request, CancellationToken cancellationToken)
        {           
            var userGroup = await _userGroupRepository.GetByIdAsync(request.Id);
            if (userGroup is null)
            {
                throw new ValidationException("UserGroup not found");
               
            }            
            var userGroupDto = _mapper.Map<UserGroupDto>(userGroup);
                
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: userGroupDto.GroupCode ?? string.Empty,        
                actionName: userGroupDto.GroupName ?? string.Empty,                
                details: $"UserGroup '{userGroupDto.GroupName}' was created. UserGroupCode: {userGroupDto.GroupCode}",
                module:"UserGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return userGroupDto;           
        }
    }
}