using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.Repacking.Commands.CreateRepacking
{
    public class CreateRepackingCommandHandler : IRequestHandler<CreateRepackingCommand, ApiResponseDTO<int>>
    {
        private readonly IRepackingCommandRepository _commandRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public CreateRepackingCommandHandler(
            IRepackingCommandRepository commandRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IMediator mediator,
            IMapper mapper,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _mediator = mediator;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateRepackingCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<RepackingHeader>(request);
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            entity.UnitId = unitId;

            // Generate RepackingNo from DocumentSequence (same pattern as CreateProductionCommandHandler)
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeRePackMaster, MiscEnumEntity.ModuleSales, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'RePackEntry' not found for Sales module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var repackingNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.RepackingNo = repackingNo
                ?? throw new ExceptionRules("No document sequence configured for RePackEntry.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "REPACKING_CREATE",
                actionName: entity.RepackingNo,
                details: $"Repacking '{entity.RepackingNo}' created successfully with Id {newId}.",
                module: "Production"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Repacking created successfully.",
                Data = newId
            };
        }
    }
}
