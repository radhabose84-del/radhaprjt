
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Inventory;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;


namespace UserManagement.Application.UserRole.Queries.GetRole
{
    public class GetRoleQueryHandler : IRequestHandler<GetRoleQuery, ApiResponseDTO<List<GetUserRoleDto>>>
    {
        private readonly IUserRoleQueryRepository _userRoleRepository;
        private readonly IRoleItemGroupMappingQueryRepository _mappingQueryRepository;
        private readonly IItemGroupLookup _itemGroupLookup;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<GetRoleQueryHandler> _logger;

        public GetRoleQueryHandler(
            IUserRoleQueryRepository userRoleRepository,
            IRoleItemGroupMappingQueryRepository mappingQueryRepository,
            IItemGroupLookup itemGroupLookup,
            IMapper mapper,
            IMediator mediator,
            ILogger<GetRoleQueryHandler> logger)
        {
            _userRoleRepository = userRoleRepository;
            _mappingQueryRepository = mappingQueryRepository;
            _itemGroupLookup = itemGroupLookup;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponseDTO<List<GetUserRoleDto>>> Handle(GetRoleQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching User Role Request started: {request}", request);

            // Fetch user roles with pagination and search
            var (roles, totalCount) = await _userRoleRepository.GetAllRoleAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            if (roles is null || !roles.Any())
            {
                _logger.LogWarning("No User Role records found in the database. Total count: {totalCount}", totalCount);

                return new ApiResponseDTO<List<GetUserRoleDto>>
                {
                    IsSuccess = false,
                    Message = "No Record Found"
                };
            }

            _logger.LogInformation("Mapping user roles to DTO.");
            var roleList = _mapper.Map<List<GetUserRoleDto>>(roles);

            // Fetch RoleItemGroupMappings for all roles in the page
            var roleIds = roleList.Select(r => r.Id).ToList();
            var allItemGroupIds = new HashSet<int>();
            var mappingsByRole = new Dictionary<int, List<Domain.Entities.RoleItemGroupMapping>>();

            foreach (var roleId in roleIds)
            {
                var mappings = await _mappingQueryRepository.GetByRoleIdAsync(roleId);
                if (mappings is not null && mappings.Count > 0)
                {
                    mappingsByRole[roleId] = mappings;
                    foreach (var m in mappings)
                        allItemGroupIds.Add(m.ItemGroupId);
                }
            }

            // Fetch ItemGroup names once for all
            Dictionary<int, string> itemGroupDict = new();
            if (allItemGroupIds.Count > 0)
            {
                var itemGroups = await _itemGroupLookup.GetItemGroupsByIdsAsync(allItemGroupIds, cancellationToken);
                itemGroupDict = itemGroups.ToDictionary(ig => ig.Id, ig => ig.ItemGroupName);
            }

            // Populate mappings on each role DTO
            foreach (var roleDto in roleList)
            {
                if (mappingsByRole.TryGetValue(roleDto.Id, out var roleMappings))
                {
                    roleDto.RoleItemGroupMappings = roleMappings.Select(m => new RoleItemGroupMappingOutputDto
                    {
                        Id = m.Id,
                        ItemGroupId = m.ItemGroupId,
                        ItemGroupName = itemGroupDict.TryGetValue(m.ItemGroupId, out var name) ? name : null
                    }).ToList();
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "User Role details were fetched.",
                module: "UserRole"
            );

            _logger.LogInformation("Publishing AuditLogsDomainEvent.");
            await _mediator.Publish(domainEvent, cancellationToken);

            _logger.LogInformation("User Role records listed successfully. Count: {Count}", roleList.Count);

            return new ApiResponseDTO<List<GetUserRoleDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = roleList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
