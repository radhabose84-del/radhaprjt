using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Domain.Events;

namespace UserManagement.Application.RoleItemGroupMapping.Queries.GetAllRoleItemGroupMapping
{
    public class GetAllRoleItemGroupMappingQueryHandler
        : IRequestHandler<GetAllRoleItemGroupMappingQuery, ApiResponseDTO<List<RoleItemGroupMappingDto>>>
    {
        private readonly IRoleItemGroupMappingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IItemGroupLookup _itemGroupLookup;

        public GetAllRoleItemGroupMappingQueryHandler(
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

        public async Task<ApiResponseDTO<List<RoleItemGroupMappingDto>>> Handle(
            GetAllRoleItemGroupMappingQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var dtos = _mapper.Map<List<RoleItemGroupMappingDto>>(data);

            // Populate RoleName from same-module navigation
            foreach (var dto in dtos)
            {
                var entity = data.FirstOrDefault(e => e.Id == dto.Id);
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
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "RoleItemGroupMapping details were fetched.",
                module: "RoleItemGroupMapping"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<RoleItemGroupMappingDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
