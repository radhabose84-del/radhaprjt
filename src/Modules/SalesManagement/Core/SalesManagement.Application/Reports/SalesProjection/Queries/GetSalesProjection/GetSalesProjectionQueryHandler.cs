using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesProjection;
using SalesManagement.Application.Reports.SalesProjection.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Reports.SalesProjection.Queries.GetSalesProjection
{
    public class GetSalesProjectionQueryHandler
        : IRequestHandler<GetSalesProjectionQuery, ApiResponseDTO<SalesProjectionDto>>
    {
        private readonly ISalesProjectionRepository _repository;
        private readonly IMediator _mediator;

        public GetSalesProjectionQueryHandler(
            ISalesProjectionRepository repository,
            IMediator mediator)
        {
            _repository = repository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<SalesProjectionDto>> Handle(
            GetSalesProjectionQuery request,
            CancellationToken cancellationToken)
        {
            var data = await _repository.GetProjectionAsync(
                request.PeriodType,
                request.DateFrom,
                request.DateTo,
                cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetSalesProjection",
                actionCode: "Get",
                actionName: data.Periods.Count.ToString(),
                details: "Sales projection report was fetched.",
                module: "SalesProjection"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<SalesProjectionDto>
            {
                IsSuccess = true,
                Message = "Sales projection report retrieved successfully.",
                Data = data
            };
        }
    }
}
