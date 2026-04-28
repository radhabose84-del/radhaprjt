using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationAmendmentById
{
    public class GetSalesQuotationAmendmentByIdQueryHandler
        : IRequestHandler<GetSalesQuotationAmendmentByIdQuery, ApiResponseDTO<List<SalesQuotationAmendmentHeaderDto>>>
    {
        private readonly ISalesQuotationAmendmentQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSalesQuotationAmendmentByIdQueryHandler(
            ISalesQuotationAmendmentQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesQuotationAmendmentHeaderDto>>> Handle(
            GetSalesQuotationAmendmentByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetBySalesQuotationHeaderIdAsync(request.SalesQuotationHeaderId);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetBySalesQuotationHeaderId",
                actionCode: "GetSalesQuotationAmendmentByIdQuery",
                actionName: request.SalesQuotationHeaderId.ToString(),
                details: $"Sales Quotation Amendments for SQ Id {request.SalesQuotationHeaderId} fetched. Count: {result.Count}.",
                module: "SalesQuotationAmendment"), cancellationToken);

            return new ApiResponseDTO<List<SalesQuotationAmendmentHeaderDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = result
            };
        }
    }
}
