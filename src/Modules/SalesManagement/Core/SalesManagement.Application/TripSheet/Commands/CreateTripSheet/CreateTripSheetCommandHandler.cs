using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.TripSheet.Commands.CreateTripSheet
{
    public class CreateTripSheetCommandHandler : IRequestHandler<CreateTripSheetCommand, ApiResponseDTO<int>>
    {
        private readonly ITripSheetCommandRepository _commandRepository;
        private readonly ITripSheetQueryRepository _queryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateTripSheetCommandHandler(
            ITripSheetCommandRepository commandRepository,
            ITripSheetQueryRepository queryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateTripSheetCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.TripSheetHeader>(request);

            // Set UnitId from JWT token (not from payload)
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            entity.UnitId = unitId;

            // Generate TripSheetNo from DocumentSequence
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeTripSheet, MiscEnumEntity.ModuleSales, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Trip Sheet' not found for Sales module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var tripSheetNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.TripSheetNo = tripSheetNo
                ?? throw new ExceptionRules("No document sequence configured for Trip Sheet.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "TRIPSHEET_CREATE",
                actionName: tripSheetNo,
                details: $"Trip Sheet '{tripSheetNo}' created successfully with Id {newId}.",
                module: "TripSheet"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Trip Sheet created successfully.",
                Data = newId
            };
        }
    }
}
