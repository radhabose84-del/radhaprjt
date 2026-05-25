using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProductionPack.Commands.StockClose
{
    public class StockCloseCommandHandler
        : IRequestHandler<StockCloseCommand, ApiResponseDTO<int>>
    {
        private readonly IProductionCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public StockCloseCommandHandler(
            IProductionCommandRepository commandRepository,
            IMediator mediator,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            StockCloseCommand request,
            CancellationToken cancellationToken)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            if (unitId == 0)
                throw new ExceptionRules("Unit could not be determined from the current session.");

            var closedCount = await _commandRepository.StockCloseAsync(
                request.ClosingDate, unitId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "StockClose",
                actionCode: "STOCK_CLOSE",
                actionName: request.ClosingDate.ToString("yyyy-MM-dd"),
                details: $"Stock closed up to {request.ClosingDate:yyyy-MM-dd} for Unit {unitId}. {closedCount} entries closed.",
                module: "Production"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = $"Stock closed successfully. {closedCount} entries updated.",
                Data = closedCount
            };
        }
    }
}
