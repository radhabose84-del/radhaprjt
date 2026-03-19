using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Domain.Events;

namespace UserManagement.Application.RoleItemGroupMapping.Queries.GetRoleItemGroupMappingByRoleId
{
    public class GetRoleItemGroupMappingByRoleIdQueryHandler
        : IRequestHandler<GetRoleItemGroupMappingByRoleIdQuery, List<RoleItemGroupMappingLookupDto>>
    {
        private readonly IRoleItemGroupMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IItemGroupLookup _itemGroupLookup;

        public GetRoleItemGroupMappingByRoleIdQueryHandler(
            IRoleItemGroupMappingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator,
            IItemGroupLookup itemGroupLookup)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _itemGroupLookup = itemGroupLookup;
        }

        public async Task<List<RoleItemGroupMappingLookupDto>> Handle(
            GetRoleItemGroupMappingByRoleIdQuery request,
            CancellationToken cancellationToken)
        {
            var entities = await _queryRepository.GetByRoleIdAsync(request.RoleId);
            var dtos = _mapper.Map<List<RoleItemGroupMappingLookupDto>>(entities);

            // Populate RoleName from same-module navigation
            foreach (var dto in dtos)
            {
                var entity = entities.FirstOrDefault(e => e.Id == dto.Id);
                if (entity?.UserRole != null)
                {
                    dto.RoleName = entity.UserRole.RoleName;
                }
            }

            // Populate ItemGroupName from cross-module lookup
            var itemGroups = await _itemGroupLookup.GetAllItemGroupsAsync(cancellationToken);
            var itemGroupDict = itemGroups.ToDictionary(ig => ig.Id, ig => ig.ItemGroupName);
            foreach (var dto in dtos)
            {
                if (itemGroupDict.TryGetValue(dto.ItemGroupId, out var name))
                {
                    dto.ItemGroupName = name;
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByRoleId",
                actionCode: request.RoleId.ToString(),
                actionName: "RoleItemGroupMapping",
                details: $"RoleItemGroupMapping for RoleId {request.RoleId} was fetched.",
                module: "RoleItemGroupMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
