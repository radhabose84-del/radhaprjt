using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesQuotation.Queries.GetAllSalesQuotationAmendment
{
    public class GetAllSalesQuotationAmendmentQueryHandler
        : IRequestHandler<GetAllSalesQuotationAmendmentQuery, ApiResponseDTO<List<SalesQuotationAmendmentHeaderDto>>>
    {
        private readonly ISalesQuotationAmendmentQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllSalesQuotationAmendmentQueryHandler(
            ISalesQuotationAmendmentQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesQuotationAmendmentHeaderDto>>> Handle(
            GetAllSalesQuotationAmendmentQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesQuotationAmendmentQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Sales Quotation Amendment list fetched.",
                module: "SalesQuotationAmendment"), cancellationToken);

            return new ApiResponseDTO<List<SalesQuotationAmendmentHeaderDto>>
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
