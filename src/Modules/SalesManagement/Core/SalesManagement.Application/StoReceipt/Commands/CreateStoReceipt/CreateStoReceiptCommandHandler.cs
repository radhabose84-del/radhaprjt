using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoReceipt.Commands.CreateStoReceipt
{
    public class CreateStoReceiptCommandHandler : IRequestHandler<CreateStoReceiptCommand, ApiResponseDTO<int>>
    {
        private readonly IStoReceiptCommandRepository _commandRepository;
        private readonly IStoReceiptQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateStoReceiptCommandHandler(
            IStoReceiptCommandRepository commandRepository,
            IStoReceiptQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateStoReceiptCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<StoReceiptHeader>(request);

            // Set Pending status
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StoReceiptLineStatus, MiscEnumEntity.StoReceiptStatusPending);
            entity.StatusId = pendingStatus?.Id ?? 0;

            // Set Pending line status for all details
            if (entity.StoReceiptDetails != null)
            {
                foreach (var detail in entity.StoReceiptDetails)
                {
                    detail.LineStatusId = entity.StatusId;
                }
            }

            // Generate auto-number: STOR-{ReceivingPlantId}-{Seq:D5}
            var stoReceiptNumber = await _commandRepository.GenerateNextStoReceiptNumberAsync(
                request.ReceivingPlantId, cancellationToken);
            entity.StoReceiptNumber = stoReceiptNumber;

            // Resolve StockLedger status IDs
            var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Packed);
            var packedStatusId = packedStatus?.Id ?? 0;

            var reservedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Reserved);
            var reservedStatusId = reservedStatus?.Id ?? 0;

            var dispatchedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Dispatched);
            var dispatchedStatusId = dispatchedStatus?.Id ?? 0;

            var newId = await _commandRepository.CreateAsync(entity, packedStatusId, reservedStatusId, dispatchedStatusId);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "STORECEIPT_CREATE",
                actionName: stoReceiptNumber,
                details: $"STO Receipt '{stoReceiptNumber}' created successfully with Id {newId}.",
                module: "StoReceipt");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "STO Receipt created successfully.",
                Data = newId
            };
        }
    }
}
