using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeAllocation.Command.UpdateBarcodeAllocation
{
    public class UpdateBarcodeAllocationCommandHandler : IRequestHandler<UpdateBarcodeAllocationCommand, ApiResponseDTO<int>>
    {
        private readonly IBarcodeAllocationCommandRepository _commandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdateBarcodeAllocationCommandHandler(
            IBarcodeAllocationCommandRepository commandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateBarcodeAllocationCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.BarcodeAllocation>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "BARCODEALLOCATION_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Barcode allocation with Id {request.Id} updated successfully.",
                module: "BarcodeAllocation"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Barcode allocation updated successfully.",
                Data = result
            };
        }
    }
}
