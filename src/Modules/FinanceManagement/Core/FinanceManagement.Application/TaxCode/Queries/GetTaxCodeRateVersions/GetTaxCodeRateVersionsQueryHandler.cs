using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxCodeRateVersions
{
    public class GetTaxCodeRateVersionsQueryHandler : IRequestHandler<GetTaxCodeRateVersionsQuery, ApiResponseDTO<List<TaxCodeRateVersionDto>>>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetTaxCodeRateVersionsQueryHandler(ITaxCodeQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<TaxCodeRateVersionDto>>> Handle(GetTaxCodeRateVersionsQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetRateVersionsAsync(request.TaxCodeId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetTaxCodeRateVersionsQuery",
                actionCode: "Get",
                actionName: request.TaxCodeId.ToString(),
                details: $"Rate versions for Tax Code Id {request.TaxCodeId} were fetched.",
                module: "TaxCodeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<TaxCodeRateVersionDto>>
            {
                IsSuccess = true,
                Message = "Rate versions retrieved successfully.",
                Data = data,
                TotalCount = data.Count
            };
        }
    }
}
