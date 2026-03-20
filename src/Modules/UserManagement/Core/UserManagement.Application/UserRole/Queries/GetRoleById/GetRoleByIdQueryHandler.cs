#nullable disable
using UserManagement.Application.UserRole.Queries.GetRole;
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using Contracts.Interfaces.Lookups.Inventory;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace UserManagement.Application.UserRole.Queries.GetRoleById
{
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, GetUserRoleDto>
    {
        private readonly IUserRoleQueryRepository _userRoleRepository;
        private readonly IRoleItemGroupMappingQueryRepository _mappingQueryRepository;
        private readonly IItemGroupLookup _itemGroupLookup;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<GetRoleByIdQueryHandler> _logger;

        public GetRoleByIdQueryHandler(
            IUserRoleQueryRepository userRoleRepository,
            IRoleItemGroupMappingQueryRepository mappingQueryRepository,
            IItemGroupLookup itemGroupLookup,
            IMapper mapper,
            IMediator mediator,
            ILogger<GetRoleByIdQueryHandler> logger)
        {
            _userRoleRepository = userRoleRepository;
            _mappingQueryRepository = mappingQueryRepository;
            _itemGroupLookup = itemGroupLookup;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<GetUserRoleDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Processing GetRoleByIdQuery for ID: {request.Id}.");

            // Fetch the user role by ID
            var userRole = await _userRoleRepository.GetByIdAsync(request.Id);
            if (userRole is null)
            {
                _logger.LogWarning($"No user role found with ID: {request.Id}.");
                throw new ValidationException($"No user role found with ID: {request.Id}.");
            }

            _logger.LogInformation($"User role found with ID: {request.Id}. Mapping to DTO.");
            var roleDto = _mapper.Map<GetUserRoleDto>(userRole);

            // Fetch RoleItemGroupMappings for this role
            var mappings = await _mappingQueryRepository.GetByRoleIdAsync(request.Id);
            if (mappings is not null && mappings.Count > 0)
            {
                var itemGroupIds = mappings.Select(m => m.ItemGroupId).Distinct().ToList();
                var itemGroups = await _itemGroupLookup.GetItemGroupsByIdsAsync(itemGroupIds, cancellationToken);
                var itemGroupDict = itemGroups.ToDictionary(ig => ig.Id, ig => ig.ItemGroupName);

                roleDto.RoleItemGroupMappings = mappings.Select(m => new RoleItemGroupMappingOutputDto
                {
                    Id = m.Id,
                    ItemGroupId = m.ItemGroupId,
                    ItemGroupName = itemGroupDict.TryGetValue(m.ItemGroupId, out var name) ? name : null
                }).ToList();
            }

            // Publish domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: roleDto.RoleName,
                actionName: roleDto.RoleName,
                details: $"UserRole '{roleDto.RoleName}' was fetched. RoleID: {roleDto.Id}.",
                module: "UserRole"
            );

            _logger.LogInformation($"Publishing AuditLogsDomainEvent for UserRole ID: {request.Id}.");
            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation($"Returning success response for UserRole ID: {roleDto.Id}.");
            return roleDto;
        }
    }
}
