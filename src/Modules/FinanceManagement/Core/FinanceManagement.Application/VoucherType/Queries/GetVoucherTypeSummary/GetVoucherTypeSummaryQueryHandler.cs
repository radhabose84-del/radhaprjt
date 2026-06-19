using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeSummary
{
    public class GetVoucherTypeSummaryQueryHandler : IRequestHandler<GetVoucherTypeSummaryQuery, VoucherTypeSummaryDto>
    {
        private readonly IVoucherTypeMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetVoucherTypeSummaryQueryHandler(IVoucherTypeMasterQueryRepository queryRepository, IIPAddressService ipAddressService, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<VoucherTypeSummaryDto> Handle(GetVoucherTypeSummaryQuery request, CancellationToken cancellationToken)
        {
            var summary = await _queryRepository.GetSummaryAsync(_ipAddressService.GetCompanyId());

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetSummary",
                actionCode: "GetVoucherTypeSummaryQuery",
                actionName: summary.TotalCount.ToString(),
                details: "Voucher Type summary was fetched.",
                module: "VoucherType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return summary;
        }
    }
}
