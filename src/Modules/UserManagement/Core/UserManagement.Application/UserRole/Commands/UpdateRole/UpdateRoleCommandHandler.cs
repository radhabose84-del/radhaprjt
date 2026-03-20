#nullable disable
using UserManagement.Application.UserRole.Queries.GetRole;
using MediatR;
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;
using FluentValidation;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.UserRole.Commands.UpdateRole
{
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, bool>
    {
        private readonly IUserRoleCommandRepository _IUserRoleRepository;
        private readonly IMapper _Imapper;
        private readonly IUserRoleQueryRepository _IUserRoleQueryRepository;
        private readonly IRoleItemGroupMappingCommandRepository _mappingCommandRepository;
        private readonly IRoleItemGroupMappingQueryRepository _mappingQueryRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateRoleCommandHandler> _logger;

        public UpdateRoleCommandHandler(
            IUserRoleCommandRepository roleRepository,
            IUserRoleQueryRepository userRoleQueryRepository,
            IRoleItemGroupMappingCommandRepository mappingCommandRepository,
            IRoleItemGroupMappingQueryRepository mappingQueryRepository,
            IMapper mapper,
            IMediator mediator,
            ILogger<UpdateRoleCommandHandler> logger)
        {
            _IUserRoleRepository = roleRepository;
            _Imapper = mapper;
            _IUserRoleQueryRepository = userRoleQueryRepository;
            _mappingCommandRepository = mappingCommandRepository;
            _mappingQueryRepository = mappingQueryRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Starting UpdateUserRoleCommandHandler for request: {request}");

            var userrole = await _IUserRoleQueryRepository.GetByIdAsync(request.Id);
            if (userrole == null)
            {
                _logger.LogWarning($"User Role with ID {request.Id} not found.");
                throw new ValidationException("User Role not found");
            }

            _logger.LogInformation($"User Role with ID {request.Id} retrieved successfully.");

            var userroleMap = _Imapper.Map<UserManagement.Domain.Entities.UserRole>(request);

            // Save updates to the repository
            var result = await _IUserRoleRepository.UpdateAsync(request.Id, userroleMap);

            if (result <= 0)
            {
                _logger.LogWarning($"Failed to update User Role with ID {request.Id}.");
                throw new Exception("Failed to update User Role");
            }

            var duplicateCheck = await _IUserRoleRepository.ExistsByNameupdateAsync(request.RoleName, request.Id);
            if (duplicateCheck)
            {
                throw new ValidationException(" User Role Name  Already Exists");
            }

            // Update RoleItemGroupMappings if provided (null = skip, no changes)
            if (request.RoleItemGroupMappings is not null)
            {
                var existingMappings = await _mappingQueryRepository.GetByRoleIdAsync(request.Id);
                var existingItemGroupIds = new HashSet<int>(existingMappings.Select(m => m.ItemGroupId));
                var incomingItemGroupIds = new HashSet<int>(
                    request.RoleItemGroupMappings.Where(m => m.ItemGroupId > 0).Select(m => m.ItemGroupId));

                // Soft-delete mappings that are no longer in the incoming list
                foreach (var existing in existingMappings)
                {
                    if (!incomingItemGroupIds.Contains(existing.ItemGroupId))
                    {
                        existing.IsDeleted = IsDelete.Deleted;
                        await _mappingCommandRepository.DeleteAsync(existing.Id, existing);
                    }
                }

                // Create new mappings that don't exist yet
                foreach (var itemGroupId in incomingItemGroupIds)
                {
                    if (!existingItemGroupIds.Contains(itemGroupId))
                    {
                        var mappingEntity = new UserManagement.Domain.Entities.RoleItemGroupMapping
                        {
                            RoleId = request.Id,
                            ItemGroupId = itemGroupId,
                            IsActive = Status.Active,
                            IsDeleted = IsDelete.NotDeleted
                        };
                        await _mappingCommandRepository.CreateAsync(mappingEntity);
                    }
                }

                _logger.LogInformation($"RoleItemGroupMappings updated for UserRole ID: {request.Id}");
            }

            _logger.LogInformation($"User Role with ID {request.Id} updated successfully.");

            // Map the updated entity to DTO
            var role = await _IUserRoleQueryRepository.GetByIdAsync(request.Id);
            var roleDto = _Imapper.Map<UserRoleDto>(role);

            // Publish domain event for audit logs
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: userrole.RoleName,
                actionName: userrole.RoleName,
                details: $"User Role '{userrole.RoleName}' was updated. User Role ID: {request.Id}",
                module: "User Role"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"AuditLogsDomainEvent published for User Role ID {userrole.Id}.");

            return result > 0;
        }
    }
}
