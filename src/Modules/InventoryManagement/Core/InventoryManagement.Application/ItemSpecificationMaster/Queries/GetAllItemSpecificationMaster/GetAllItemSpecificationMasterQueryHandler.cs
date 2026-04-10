
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationMaster.Queries.GetAllItemSpecificationMaster
{
    public class GetAllItemSpecificationMasterQueryHandler : IRequestHandler<GetAllItemSpecificationMasterQuery, ApiResponseDTO<List<ItemSpecificationMasterDto>>>
    {
        private readonly IItemSpecificationMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllItemSpecificationMasterQueryHandler(
            IItemSpecificationMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ItemSpecificationMasterDto>>> Handle(GetAllItemSpecificationMasterQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm);

            var dtos = _mapper.Map<List<ItemSpecificationMasterDto>>(data);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllItemSpecificationMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Item Specification Master details were fetched.",
                module: "ItemSpecificationMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ItemSpecificationMasterDto>>
            {
                IsSuccess = true,
                Message = "Item Specification Masters retrieved successfully",
                Data = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
