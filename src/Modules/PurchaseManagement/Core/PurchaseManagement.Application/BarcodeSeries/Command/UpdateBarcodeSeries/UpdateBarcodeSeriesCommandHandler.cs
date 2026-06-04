using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeSeries.Command.UpdateBarcodeSeries
{
    public class UpdateBarcodeSeriesCommandHandler : IRequestHandler<UpdateBarcodeSeriesCommand, ApiResponseDTO<int>>
    {
        private readonly IBarcodeSeriesCommandRepository _commandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdateBarcodeSeriesCommandHandler(
            IBarcodeSeriesCommandRepository commandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateBarcodeSeriesCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.BarcodeSeries>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "BARCODESERIES_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Barcode series with Id {request.Id} updated successfully.",
                module: "BarcodeSeries"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Barcode series updated successfully.",
                Data = result
            };
        }
    }
}
