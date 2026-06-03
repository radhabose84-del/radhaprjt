using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeSeries.Command.CreateBarcodeSeries
{
    public class CreateBarcodeSeriesCommandHandler : IRequestHandler<CreateBarcodeSeriesCommand, ApiResponseDTO<int>>
    {
        private readonly IBarcodeSeriesCommandRepository _commandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateBarcodeSeriesCommandHandler(
            IBarcodeSeriesCommandRepository commandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateBarcodeSeriesCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.BarcodeSeries>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "BARCODESERIES_CREATE",
                actionName: entity.BarcodeSeriesNumber ?? newId.ToString(),
                details: $"Barcode series '{entity.BarcodeSeriesNumber}' created successfully with Id {newId}.",
                module: "BarcodeSeries"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Barcode series created successfully.",
                Data = newId
            };
        }
    }
}
