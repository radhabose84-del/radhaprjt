using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesAgreement.Queries.GetAllSalesAgreement
{
    public class GetAllSalesAgreementQueryHandler : IRequestHandler<GetAllSalesAgreementQuery, ApiResponseDTO<List<SalesAgreementHeaderDto>>>
    {
        private readonly ISalesAgreementQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAllSalesAgreementQueryHandler(
            ISalesAgreementQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<SalesAgreementHeaderDto>>> Handle(GetAllSalesAgreementQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllSalesAgreementQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Sales Agreement details were fetched.",
                module: "SalesAgreement");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<SalesAgreementHeaderDto>>
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
