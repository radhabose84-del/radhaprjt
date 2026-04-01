using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingMaster.Commands.CreateRepackingMaster
{
    public class CreateRepackingMasterCommandHandler
        : IRequestHandler<CreateRepackingMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IRepackingMasterCommandRepository _commandRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public CreateRepackingMasterCommandHandler(
            IRepackingMasterCommandRepository commandRepository,
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

        public async Task<ApiResponseDTO<int>> Handle(
            CreateRepackingMasterCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.RepackingMaster>(request);
            entity.UnitId = _ipAddressService.GetUnitId() ?? 0;
            entity.ProductionYear = request.RepackDate.Year;

            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // Generate RepackDocNo from DocumentSequence
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeRePackMaster, MiscEnumEntity.ModuleSales, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'RePackEntry' not found for Sales module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var repackDocNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.RepackDocNo = repackDocNo
                ?? throw new ExceptionRules("No document sequence configured for RePackEntry.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "REPACKING_CREATE",
                actionName: entity.RepackDocNo,
                details: $"Repacking '{entity.RepackDocNo}' created successfully with Id {newId}.",
                module: "RepackingMaster"
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
