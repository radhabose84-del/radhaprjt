using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeAllocation.Command.CreateBarcodeAllocation
{
    public class CreateBarcodeAllocationCommandHandler : IRequestHandler<CreateBarcodeAllocationCommand, ApiResponseDTO<int>>
    {
        private readonly IBarcodeAllocationCommandRepository _commandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateBarcodeAllocationCommandHandler(
            IBarcodeAllocationCommandRepository commandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateBarcodeAllocationCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.BarcodeAllocation>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "BARCODEALLOCATION_CREATE",
                actionName: entity.AllocationNumber ?? newId.ToString(),
                details: $"Barcode allocation '{entity.AllocationNumber}' created successfully with Id {newId}.",
                module: "BarcodeAllocation"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Barcode allocation created successfully.",
                Data = newId
            };
        }
    }
}
