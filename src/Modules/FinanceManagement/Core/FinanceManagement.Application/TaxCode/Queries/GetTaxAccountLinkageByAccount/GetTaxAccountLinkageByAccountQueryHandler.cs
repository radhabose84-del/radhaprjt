using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxAccountLinkageByAccount
{
    public class GetTaxAccountLinkageByAccountQueryHandler : IRequestHandler<GetTaxAccountLinkageByAccountQuery, ApiResponseDTO<TaxAccountLinkageDto?>>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetTaxAccountLinkageByAccountQueryHandler(ITaxCodeQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<TaxAccountLinkageDto?>> Handle(GetTaxAccountLinkageByAccountQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetLinkageByAccountAsync(request.GlAccountId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByAccount",
                actionCode: "GetTaxAccountLinkageByAccountQuery",
                actionName: request.GlAccountId.ToString(),
                details: $"Active tax-account linkage for GlAccountId {request.GlAccountId} was fetched.",
                module: "TaxAccountLinkage"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<TaxAccountLinkageDto?>
            {
                IsSuccess = data != null,
                Message = data != null ? "Success" : "No active linkage found for the given account.",
                Data = data
            };
        }
    }
}
