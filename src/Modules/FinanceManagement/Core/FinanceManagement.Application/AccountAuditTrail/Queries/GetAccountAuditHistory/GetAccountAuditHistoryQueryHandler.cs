using Contracts.Interfaces;
using FinanceManagement.Application.AccountAuditTrail.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountAuditTrail;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountAuditTrail.Queries.GetAccountAuditHistory
{
    public class GetAccountAuditHistoryQueryHandler
        : IRequestHandler<GetAccountAuditHistoryQuery, List<AccountAuditTrailDto>>
    {
        private readonly IAccountAuditTrailQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetAccountAuditHistoryQueryHandler(
            IAccountAuditTrailQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<List<AccountAuditTrailDto>> Handle(
            GetAccountAuditHistoryQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId() ?? 0;

            var rows = await _queryRepository.GetHistoryAsync(
                companyId, request.EntityName, request.EntityId, cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAccountAuditHistoryQuery",
                actionCode: "Get",
                actionName: $"{request.EntityName}#{request.EntityId}",
                details: $"Account audit history fetched ({rows.Count} rows) for {request.EntityName} Id {request.EntityId}.",
                module: "AccountAuditTrail"), cancellationToken);

            return rows;
        }
    }
}
