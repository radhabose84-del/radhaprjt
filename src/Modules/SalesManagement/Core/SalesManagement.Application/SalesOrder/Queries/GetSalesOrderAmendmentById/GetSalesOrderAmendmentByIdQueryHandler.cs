using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Queries.GetSalesOrderAmendmentById
{
    public class GetSalesOrderAmendmentByIdQueryHandler
        : IRequestHandler<GetSalesOrderAmendmentByIdQuery, ApiResponseDTO<SalesOrderAmendmentHeaderDto>>
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

        public async Task<ApiResponseDTO<SalesOrderAmendmentHeaderDto>> Handle(
            GetSalesOrderAmendmentByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);
            if (result == null)
                return new ApiResponseDTO<SalesOrderAmendmentHeaderDto>
                {
                    IsSuccess = false,
                    Message = "Sales Order Amendment not found.",
                    Data = null
                };

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesOrderAmendmentByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Sales Order Amendment Id {result.Id} fetched.",
                module: "SalesOrderAmendment"), cancellationToken);

            return new ApiResponseDTO<SalesOrderAmendmentHeaderDto>
            {
                IsSuccess = true,
                Message = "Success",
                Data = result
            };
        }
    }
}
