using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceByItemAndDate
{
    public class GetItemPriceByItemAndDateQueryHandler
        : IRequestHandler<GetItemPriceByItemAndDateQuery, ApiResponseDTO<List<ItemPriceMasterDto>>>
    {
        private readonly IItemPriceMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetItemPriceByItemAndDateQueryHandler(
            IItemPriceMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ItemPriceMasterDto>>> Handle(
            GetItemPriceByItemAndDateQuery request,
            CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByItemAndDateAsync(
                request.ItemId, request.Date);

            var dtos = _mapper.Map<List<ItemPriceMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetItemPriceByItemAndDateQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: $"ItemPriceMaster details fetched for ItemId {request.ItemId} on date {request.Date}.",
                module: "ItemPriceMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ItemPriceMasterDto>>
            {
                IsSuccess = true,
                Message = "Item price details retrieved successfully.",
                Data = dtos,
                TotalCount = data.Count
            };
        }
    }
}
