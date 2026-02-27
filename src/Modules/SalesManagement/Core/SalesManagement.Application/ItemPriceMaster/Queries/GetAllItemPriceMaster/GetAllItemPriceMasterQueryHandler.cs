using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ItemPriceMaster.Queries.GetAllItemPriceMaster
{
    public class GetAllItemPriceMasterQueryHandler
        : IRequestHandler<GetAllItemPriceMasterQuery, ApiResponseDTO<List<ItemPriceMasterDto>>>
    {
        private readonly IItemPriceMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllItemPriceMasterQueryHandler(IItemPriceMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ItemPriceMasterDto>>> Handle(
            GetAllItemPriceMasterQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var salesItemPriceMasterDtos = _mapper.Map<List<ItemPriceMasterDto>>(data);

            // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllItemPriceMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "ItemPriceMaster details were fetched.",
                module: "ItemPriceMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ItemPriceMasterDto>>
            {
                IsSuccess = true,
                Message = "Item Price Masters retrieved successfully.",
                Data = salesItemPriceMasterDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
