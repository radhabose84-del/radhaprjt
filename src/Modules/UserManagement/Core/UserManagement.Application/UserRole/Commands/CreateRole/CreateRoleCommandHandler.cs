#nullable disable
using UserManagement.Application.UserRole.Queries.GetRole;
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;
using FluentValidation;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.UserRole.Commands.CreateRole
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, UserRoleDto>
    {
        private readonly IUserRoleCommandRepository _roleRepository;
        private readonly IUserRoleQueryRepository _userRoleQueryRepository;
        private readonly IRoleItemGroupMappingCommandRepository _mappingCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateRoleCommandHandler> _logger;

        public CreateRoleCommandHandler(
            IUserRoleCommandRepository roleRepository,
            IUserRoleQueryRepository userRoleQueryRepository,
            IRoleItemGroupMappingCommandRepository mappingCommandRepository,
            IMapper mapper,
            IMediator mediator,
            ILogger<CreateRoleCommandHandler> logger)
        {
            _roleRepository = roleRepository;
            _userRoleQueryRepository = userRoleQueryRepository;
            _mappingCommandRepository = mappingCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<UserRoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting CreateUserRoleCommandHandler for request: {request}");

            var exists = await _roleRepository.ExistsByCodeAsync(request.RoleName);
            if (exists)
            {
                _logger.LogWarning($"Entity Name {request.RoleName} already exists.");
                throw new ValidationException("UserRole Name already exists.");
            }

            // Map the request to the entity
            var userRoleEntity = _mapper.Map<UserManagement.Domain.Entities.UserRole>(request);
            _logger.LogInformation($"Mapped CreateUserRoleCommand to userRole entity:{userRoleEntity}");

            // Save the UserRole
            var createdUserRole = await _roleRepository.CreateAsync(userRoleEntity);

            if (createdUserRole is null)
            {
                _logger.LogWarning($"Failed to create UserRole. UserRole entity: {userRoleEntity}");
                throw new Exception("UserRole not created");
            }

            _logger.LogInformation($"UserRole successfully created with ID: {createdUserRole.Id}");

            // Create RoleItemGroupMappings if provided (null = skip)
            if (request.RoleItemGroupMappings is not null)
            {
                foreach (var mappingInput in request.RoleItemGroupMappings.Where(m => m.ItemGroupId > 0))
                {
                    var mappingEntity = new UserManagement.Domain.Entities.RoleItemGroupMapping
                    {
                        RoleId = createdUserRole.Id,
                        ItemGroupId = mappingInput.ItemGroupId,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    };
                    await _mappingCommandRepository.CreateAsync(mappingEntity);
                }

                _logger.LogInformation($"RoleItemGroupMappings created for UserRole ID: {createdUserRole.Id}");
            }

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
            var userRoleDto = _mapper.Map<UserRoleDto>(createdUserRole);

            _logger.LogInformation($"Returning success response for UserRole ID: {createdUserRole.Id}");

            return userRoleDto;
        }
    }
}
