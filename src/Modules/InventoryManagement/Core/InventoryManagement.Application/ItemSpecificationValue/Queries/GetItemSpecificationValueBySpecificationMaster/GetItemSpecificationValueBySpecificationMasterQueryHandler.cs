using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueBySpecificationMaster
{
    public class GetItemSpecificationValueBySpecificationMasterQueryHandler
        : IRequestHandler<GetItemSpecificationValueBySpecificationMasterQuery, ApiResponseDTO<List<ItemSpecificationValueDto>>>
    {
        private readonly IItemSpecificationValueQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetItemSpecificationValueBySpecificationMasterQueryHandler(
            IItemSpecificationValueQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ItemSpecificationValueDto>>> Handle(
            GetItemSpecificationValueBySpecificationMasterQuery request,
            CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetBySpecificationMasterIdAsync(request.SpecificationMasterId, cancellationToken);
            var dtos = _mapper.Map<List<ItemSpecificationValueDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetItemSpecificationValueBySpecificationMasterQuery",
                actionCode: "Get",
                actionName: dtos.Count.ToString(),
                details: $"Item Specification Values filtered by SpecificationMasterId {request.SpecificationMasterId} were fetched.",
                module: "ItemSpecificationValue"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ItemSpecificationValueDto>>
            {
                IsSuccess = true,
                Message = "Item Specification Values retrieved successfully",
                Data = dtos,
                TotalCount = dtos.Count
            };
        }
    }
}
