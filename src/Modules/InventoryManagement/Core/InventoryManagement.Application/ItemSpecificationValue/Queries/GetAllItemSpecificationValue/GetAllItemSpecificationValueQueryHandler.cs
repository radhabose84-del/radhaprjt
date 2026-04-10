
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationValue.Queries.GetAllItemSpecificationValue
{
    public class GetAllItemSpecificationValueQueryHandler : IRequestHandler<GetAllItemSpecificationValueQuery, ApiResponseDTO<List<ItemSpecificationValueDto>>>
    {
        private readonly IItemSpecificationValueQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllItemSpecificationValueQueryHandler(
            IItemSpecificationValueQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ItemSpecificationValueDto>>> Handle(GetAllItemSpecificationValueQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm);

            var dtos = _mapper.Map<List<ItemSpecificationValueDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllItemSpecificationValueQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Item Specification Value details were fetched.",
                module: "ItemSpecificationValue"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ItemSpecificationValueDto>>
            {
                IsSuccess = true,
                Message = "Item Specification Values retrieved successfully",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
