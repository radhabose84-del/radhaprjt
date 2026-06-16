using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetLinkageChangeAudit
{
    public class GetLinkageChangeAuditQueryHandler : IRequestHandler<GetLinkageChangeAuditQuery, ApiResponseDTO<List<TaxAccountLinkageDto>>>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetLinkageChangeAuditQueryHandler(ITaxCodeQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<TaxAccountLinkageDto>>> Handle(GetLinkageChangeAuditQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetLinkageChangeAuditAsync(request.Status, request.CompanyId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetLinkageChangeAuditQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Tax linkage change audit was fetched.",
                module: "TaxAccountLinkage"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<TaxAccountLinkageDto>>
            {
                IsSuccess = true,
                Message = "Change audit retrieved successfully.",
                Data = data,
                TotalCount = data.Count
            };
        }
    }
}
