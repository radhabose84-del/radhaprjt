using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Domain.Events;

namespace UserManagement.Application.RoleItemGroupMapping.Queries.GetRoleItemGroupMappingById
{
    public class GetRoleItemGroupMappingByIdQueryHandler
        : IRequestHandler<GetRoleItemGroupMappingByIdQuery, RoleItemGroupMappingDto>
    {
        private readonly IRoleItemGroupMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IItemGroupLookup _itemGroupLookup;

        public GetRoleItemGroupMappingByIdQueryHandler(
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

        public async Task<RoleItemGroupMappingDto> Handle(
            GetRoleItemGroupMappingByIdQuery request,
            CancellationToken cancellationToken)
        {
            var entity = await _queryRepository.GetByIdAsync(request.Id);
            if (entity is null)
            {
                throw new ValidationException($"RoleItemGroupMapping with ID {request.Id} not found.");
            }

            var dto = _mapper.Map<RoleItemGroupMappingDto>(entity);

            // Populate RoleName from same-module navigation
            if (entity.UserRole != null)
            {
                dto.RoleName = entity.UserRole.RoleName;
            }

            // Populate ItemGroupName from cross-module lookup
            var itemGroups = await _itemGroupLookup.GetAllItemGroupsAsync(cancellationToken);
            var itemGroup = itemGroups.FirstOrDefault(ig => ig.Id == dto.ItemGroupId);
            if (itemGroup != null)
            {
                dto.ItemGroupName = itemGroup.ItemGroupName;
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: dto.Id.ToString(),
                actionName: "RoleItemGroupMapping",
                details: $"RoleItemGroupMapping Id {dto.Id} was fetched.",
                module: "RoleItemGroupMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
