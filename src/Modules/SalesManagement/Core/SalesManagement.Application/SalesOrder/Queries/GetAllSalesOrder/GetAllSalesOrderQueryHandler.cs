using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrder
{
    public class GetAllSalesOrderQueryHandler : IRequestHandler<GetAllSalesOrderQuery, ApiResponseDTO<List<SalesOrderHeaderDto>>>
    {
        private readonly ISalesOrderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllSalesOrderQueryHandler(
            ISalesOrderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesOrderHeaderDto>>> Handle(GetAllSalesOrderQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm,
                request.OrderDateFrom, request.OrderDateTo, request.PartyName, request.StatusName);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesOrderQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Sales Order details were fetched.",
                module: "SalesOrder");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesOrderHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
