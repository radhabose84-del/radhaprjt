#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesItemPriceMaster.Queries.GetAllSalesItemPriceMaster
{
    public class GetAllSalesItemPriceMasterQueryHandler
        : IRequestHandler<GetAllSalesItemPriceMasterQuery, ApiResponseDTO<List<SalesItemPriceMasterDto>>>
    {
        private readonly ISalesItemPriceMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllSalesItemPriceMasterQueryHandler(ISalesItemPriceMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesItemPriceMasterDto>>> Handle(
            GetAllSalesItemPriceMasterQuery request,
            CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            var salesItemPriceMasterDtos = _mapper.Map<List<SalesItemPriceMasterDto>>(data);

            // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesItemPriceMasterQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "SalesItemPriceMaster details were fetched.",
                module: "SalesItemPriceMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesItemPriceMasterDto>>
            {
                IsSuccess = true,
                Message = "Sales Item Price Masters retrieved successfully.",
                Data = salesItemPriceMasterDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
