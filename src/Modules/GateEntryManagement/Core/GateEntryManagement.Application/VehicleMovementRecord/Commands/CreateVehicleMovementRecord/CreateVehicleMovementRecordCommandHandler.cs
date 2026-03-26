using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Commands.CreateVehicleMovementRecord
{
    public class CreateVehicleMovementRecordCommandHandler : IRequestHandler<CreateVehicleMovementRecordCommand, ApiResponseDTO<int>>
    {
        private readonly IVehicleMovementRecordCommandRepository _commandRepository;
        private readonly IVehicleMovementRecordQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public CreateVehicleMovementRecordCommandHandler(
            IVehicleMovementRecordCommandRepository commandRepository,
            IVehicleMovementRecordQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IMediator mediator,
            IMapper mapper,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _mediator = mediator;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateVehicleMovementRecordCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.VehicleMovementRecord>(request);

            // Auto-generate Vehicle Movement ID via DocumentSequence
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeGateEntry, MiscEnumEntity.ModuleGateEntry, request.UnitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Gate Entry' not found for Gate Entry module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var vmrId = sequences.Count > 0 ? sequences[^1] : null;
            entity.VehicleMovementId = vmrId
                ?? throw new ExceptionRules("No document sequence configured for Gate Entry.");

            // Auto-capture system fields
            entity.GateInTime = DateTime.UtcNow;
            entity.GateInBy = _ipAddressService.GetUserName();
            // Default status: Inside Premises
            var statusMisc = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.VMRStatus, MiscEnumEntity.VMRStatusInsidePremises);
            entity.StatusId = statusMisc?.Id
                ?? throw new ExceptionRules("VMR Status 'Inside Premises' not found in MiscMaster.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "VMR_CREATE",
                actionName: entity.VehicleMovementId,
                details: $"Vehicle Movement Record '{entity.VehicleMovementId}' created successfully. Vehicle '{request.VehicleNumber}' is now inside premises.",
                module: "VehicleMovementRecord"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Vehicle Movement Record created successfully. Vehicle is now inside premises.",
                Data = newId
            };
        }
    }
}
