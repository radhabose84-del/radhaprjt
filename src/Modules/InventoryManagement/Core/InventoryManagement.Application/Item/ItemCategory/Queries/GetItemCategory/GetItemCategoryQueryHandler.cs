using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory
{
    public class GetItemCategoryQueryHandler : IRequestHandler<GetItemCategoryQuery, ApiResponseDTO<List<ItemCategoryDto>>>
    {
        private readonly IItemCategoryQueryRepository _itemCategoryQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetItemCategoryQueryHandler(IItemCategoryQueryRepository itemCategoryQueryRepository, IMediator mediator, IMapper mapper)
        {
            _itemCategoryQueryRepository = itemCategoryQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<ItemCategoryDto>>> Handle(GetItemCategoryQuery request, CancellationToken cancellationToken)
        {
            var (itemCategory, totalCount) = await _itemCategoryQueryRepository.GetAllItemCategoryAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var itemCategoryDto = _mapper.Map<List<ItemCategoryDto>>(itemCategory);

            // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetNotificationConfig",
                actionCode: "Get",
                actionName: itemCategory.Count().ToString(),
                details: "Notification details were fetched.",
                module: "NotificationConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // ✅ Return
            return new ApiResponseDTO<List<ItemCategoryDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = itemCategoryDto,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}