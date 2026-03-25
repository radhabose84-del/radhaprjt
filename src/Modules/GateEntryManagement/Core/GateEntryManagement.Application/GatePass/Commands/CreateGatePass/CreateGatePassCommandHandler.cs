using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.GatePass.Dto;
using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Entities;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Commands.CreateGatePass
{
    public class CreateGatePassCommandHandler : IRequestHandler<CreateGatePassCommand, ApiResponseDTO<int>>
    {
        private readonly IGatePassCommandRepository _commandRepository;
        private readonly IGatePassQueryRepository _queryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public CreateGatePassCommandHandler(
            IGatePassCommandRepository commandRepository,
            IGatePassQueryRepository queryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IMediator mediator,
            IMapper mapper,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _mediator = mediator;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateGatePassCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<GatePassHdr>(request);

            // Auto-generate Gate Pass No via DocumentSequence
            var unitId = _ipAddressService.GetUnitId() ?? request.UnitId;
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeGatePass, MiscEnumEntity.ModuleGateEntry, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Gate Pass' not found for Gate Entry module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var gatePassNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.GatePassNo = gatePassNo
                ?? throw new ExceptionRules("No document sequence configured for Gate Pass.");

            // Map detail lines
            if (request.GatePassDetails != null && request.GatePassDetails.Count > 0)
            {
                entity.GatePassDetails = _mapper.Map<List<GatePassDtl>>(request.GatePassDetails);
            }

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "GATEPASS_CREATE",
                actionName: entity.GatePassNo,
                details: $"Gate Pass '{entity.GatePassNo}' created successfully with Id {newId}.",
                module: "GatePass"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Gate Pass created successfully.",
                Data = newId
            };
        }
    }
}
