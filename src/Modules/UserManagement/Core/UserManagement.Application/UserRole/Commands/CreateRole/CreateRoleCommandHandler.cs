using UserManagement.Application.UserRole.Queries.GetRole;
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace UserManagement.Application.UserRole.Commands.CreateRole
{
    public class CreateRoleCommandHandler :IRequestHandler<CreateRoleCommand,  UserRoleDto>
    {
        
        private readonly IUserRoleCommandRepository _roleRepository;
        private readonly IUserRoleQueryRepository _userRoleQueryRepository;
        private readonly IMapper _mapper;
        
        private readonly IMediator _mediator; 
        
          private readonly ILogger<CreateRoleCommandHandler> _logger;

        public CreateRoleCommandHandler(IUserRoleCommandRepository roleRepository ,IUserRoleQueryRepository userRoleQueryRepository  ,IMapper mapper,IMediator mediator,ILogger<CreateRoleCommandHandler> logger)
        {
             _roleRepository=roleRepository;
             _userRoleQueryRepository=  userRoleQueryRepository;
            _mapper=mapper;
            _mediator=mediator;
            _logger=logger;
        }

        public async Task<UserRoleDto> Handle(CreateRoleCommand request,CancellationToken cancellationToken)
         {          
            _logger.LogInformation($"Starting CreateUserRoleCommandHandler for request: {request}");
            
            var exists = await _roleRepository.ExistsByCodeAsync(request.RoleName);
              if (exists)
                 {
                     _logger.LogWarning($"Entity Name {request.RoleName} already exists." );
                     throw new ValidationException("UserRole Name already exists.");
                       
                }
            // Map the request to the entity
            var userRoleEntity = _mapper.Map<UserManagement.Domain.Entities.UserRole>(request);
            _logger.LogInformation($"Mapped CreateUserRoleCommand to userRole entity:{userRoleEntity}" );

            // Save the UserRole
            var createdUserRole = await _roleRepository.CreateAsync(userRoleEntity);

            if (createdUserRole is null)
            {
                _logger.LogWarning($"Failed to create UserRole. UserRole entity: {userRoleEntity}");
                throw new Exception("UserRole not created");
               
            }

            _logger.LogInformation($"UserRole successfully created with ID: {createdUserRole.Id}" );

            // Publish the domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: createdUserRole.Id.ToString(),
                actionName: createdUserRole.RoleName,
                details: $"UserRole '{createdUserRole.RoleName}' was created. ID: {createdUserRole.Id}",
                module: "UserRole"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"AuditLogsDomainEvent published for UserRole ID: {createdUserRole.Id}");

            // Map the result to DTO
            var userrolrDto = _mapper.Map<UserRoleDto>(createdUserRole);

            _logger.LogInformation($"Returning success response for UserRole ID: {createdUserRole.Id}");

            return userrolrDto;
           
       


         }



    }
}