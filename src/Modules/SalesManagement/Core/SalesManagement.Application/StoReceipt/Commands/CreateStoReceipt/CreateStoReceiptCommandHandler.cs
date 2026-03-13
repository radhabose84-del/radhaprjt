using AutoMapper;
using Contracts.Common;
using MediatR;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
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
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateStoReceiptCommandHandler(
            IStoReceiptCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
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

            // Get UnitId from JWT token
            var unitId = _ipAddressService.GetUnitId();

            // Generate STO Receipt Number from Finance.DocumentSequence
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeStogr, MiscEnumEntity.ModuleSales, unitId ?? 0);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'STO Goods Receipt' not found for Sales module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var stoReceiptNumber = sequences.Count > 0 ? sequences[^1] : null;
            entity.StoReceiptNumber = stoReceiptNumber
                ?? throw new ExceptionRules("No document sequence configured for STO Goods Receipt.");

            // Resolve Packed and Damaged status IDs for new StockLedger rows at receiving plant
            var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Packed);
            var packedStatusId = packedStatus?.Id ?? 0;

            var damagedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Damaged);
            var damagedStatusId = damagedStatus?.Id ?? 0;

            var dispatchedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Dispatched);
            var dispatchedStatusId = dispatchedStatus?.Id ?? 0;

            var newId = await _commandRepository.CreateAsync(entity, packedStatusId, damagedStatusId, dispatchedStatusId, typeId.Value);

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
