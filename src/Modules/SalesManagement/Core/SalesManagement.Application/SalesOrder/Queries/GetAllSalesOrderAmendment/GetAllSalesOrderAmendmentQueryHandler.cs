using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Queries.GetAllSalesOrderAmendment
{
    public class GetAllSalesOrderAmendmentQueryHandler
        : IRequestHandler<GetAllSalesOrderAmendmentQuery, ApiResponseDTO<List<SalesOrderAmendmentHeaderDto>>>
    {
        private readonly ISalesOrderAmendmentQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllSalesOrderAmendmentQueryHandler(
            ISalesOrderAmendmentQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesOrderAmendmentHeaderDto>>> Handle(
            GetAllSalesOrderAmendmentQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesOrderAmendmentQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Sales Order Amendment list fetched.",
                module: "SalesOrderAmendment"), cancellationToken);

            return new ApiResponseDTO<List<SalesOrderAmendmentHeaderDto>>
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
