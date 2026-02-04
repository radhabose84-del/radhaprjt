using AutoMapper;
using InventoryManagement.Application.Common.HttpResponse;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup
{
    public class GetItemGroupQueryHandler : IRequestHandler<GetItemGroupQuery, ApiResponseDTO<List<ItemGroupDto>>>
    {
        private readonly IItemGroupQueryRepository _itemGroupQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetItemGroupQueryHandler(IItemGroupQueryRepository itemGroupQueryRepository, IMediator mediator, IMapper mapper)
        {
            _itemGroupQueryRepository = itemGroupQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<ItemGroupDto>>> Handle(GetItemGroupQuery request, CancellationToken cancellationToken)
        {
            var (itemGroup, totalCount) = await _itemGroupQueryRepository.GetAllItemGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var itemGroupDto = _mapper.Map<List<ItemGroupDto>>(itemGroup);

            // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetNotificationConfig",
                actionCode: "Get",
                actionName: itemGroup.Count().ToString(),
                details: "Notification details were fetched.",
                module: "NotificationConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // ✅ Return
            return new ApiResponseDTO<List<ItemGroupDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = itemGroupDto,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}