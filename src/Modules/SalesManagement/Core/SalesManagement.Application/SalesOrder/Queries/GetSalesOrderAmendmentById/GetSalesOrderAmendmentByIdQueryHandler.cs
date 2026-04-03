using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Queries.GetSalesOrderAmendmentById
{
    public class GetSalesOrderAmendmentByIdQueryHandler
        : IRequestHandler<GetSalesOrderAmendmentByIdQuery, ApiResponseDTO<List<SalesOrderAmendmentHeaderDto>>>
    {
        private readonly ISalesOrderAmendmentQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSalesOrderAmendmentByIdQueryHandler(
            ISalesOrderAmendmentQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesOrderAmendmentHeaderDto>>> Handle(
            GetSalesOrderAmendmentByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetBySalesOrderHeaderIdAsync(request.SalesOrderHeaderId);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetBySalesOrderHeaderId",
                actionCode: "GetSalesOrderAmendmentByIdQuery",
                actionName: request.SalesOrderHeaderId.ToString(),
                details: $"Sales Order Amendments for SO Id {request.SalesOrderHeaderId} fetched. Count: {result.Count}.",
                module: "SalesOrderAmendment"), cancellationToken);

            return new ApiResponseDTO<List<SalesOrderAmendmentHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = result
            };
        }
    }
}
