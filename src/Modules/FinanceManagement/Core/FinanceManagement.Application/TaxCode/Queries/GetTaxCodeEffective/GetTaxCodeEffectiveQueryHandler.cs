using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetTaxCodeEffective
{
    public class GetTaxCodeEffectiveQueryHandler : IRequestHandler<GetTaxCodeEffectiveQuery, ApiResponseDTO<TaxCodeMasterDto?>>
    {
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetTaxCodeEffectiveQueryHandler(ITaxCodeQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<TaxCodeMasterDto?>> Handle(GetTaxCodeEffectiveQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetEffectiveAsync(request.Code, request.CompanyId, request.AsOf);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetTaxCodeEffectiveQuery",
                actionCode: "Get",
                actionName: request.Code,
                details: $"Effective rate for Tax Code '{request.Code}' as of {request.AsOf:yyyy-MM-dd} was fetched.",
                module: "TaxCodeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<TaxCodeMasterDto?>
            {
                IsSuccess = data != null,
                Message = data != null ? "Success" : "No effective tax code found for the given date.",
                Data = data
            };
        }
    }
}
