using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeNumberSeries
{
    public class GetVoucherTypeNumberSeriesQueryHandler
        : IRequestHandler<GetVoucherTypeNumberSeriesQuery, ApiResponseDTO<List<VoucherTypeNumberSeriesDto>>>
    {
        private readonly IVoucherTypeMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetVoucherTypeNumberSeriesQueryHandler(IVoucherTypeMasterQueryRepository queryRepository, IIPAddressService ipAddressService, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<VoucherTypeNumberSeriesDto>>> Handle(GetVoucherTypeNumberSeriesQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetNumberSeriesAsync(request.FinancialYearId, _ipAddressService.GetCompanyId());

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetNumberSeries",
                actionCode: "GetVoucherTypeNumberSeriesQuery",
                actionName: data.Count.ToString(),
                details: $"Voucher Type number series fetched for fiscal year {request.FinancialYearId}.",
                module: "VoucherType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<VoucherTypeNumberSeriesDto>>
            {
                IsSuccess = true,
                Message = "Voucher Type number series retrieved successfully.",
                Data = data,
                TotalCount = data.Count
            };
        }
    }
}
