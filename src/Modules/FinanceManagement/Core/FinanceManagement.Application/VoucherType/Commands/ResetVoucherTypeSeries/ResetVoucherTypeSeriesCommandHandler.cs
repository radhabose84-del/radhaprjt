using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Commands.ResetVoucherTypeSeries
{
    public class ResetVoucherTypeSeriesCommandHandler : IRequestHandler<ResetVoucherTypeSeriesCommand, ApiResponseDTO<int>>
    {
        private readonly IVoucherTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public ResetVoucherTypeSeriesCommandHandler(
            IVoucherTypeMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(ResetVoucherTypeSeriesCommand request, CancellationToken cancellationToken)
        {
            var seriesId = await _commandRepository.ResetSeriesAsync(request.VoucherTypeId, request.FinancialYearId);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "VOUCHER_TYPE_SERIES_RESET",
                actionName: request.VoucherTypeId.ToString(),
                details: $"Voucher Type {request.VoucherTypeId} number series reset for fiscal year {request.FinancialYearId}.",
                module: "VoucherType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Voucher Type number series reset successfully.",
                Data = seriesId
            };
        }
    }
}
