using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IDocumentSequence;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoHeader.Commands.CreateStoHeader
{
    public class CreateStoHeaderCommandHandler : IRequestHandler<CreateStoHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IStoHeaderCommandRepository _commandRepository;
        private readonly IStoHeaderQueryRepository _queryRepository;
        private readonly IDocumentSequenceQueryRepository _documentSequenceQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateStoHeaderCommandHandler(
            IStoHeaderCommandRepository commandRepository,
            IStoHeaderQueryRepository queryRepository,
            IDocumentSequenceQueryRepository documentSequenceQueryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _documentSequenceQueryRepository = documentSequenceQueryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateStoHeaderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.StoHeader>(request);

            // Get UnitId from JWT token
            var unitId = _ipAddressService.GetUnitId();

            // Generate STO Number from Finance.DocumentSequence
            var typeId = await _documentSequenceQueryRepository.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeSto, MiscEnumEntity.ModuleSales, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Stock Transfer Order' not found for Sales module.");

            var sequences = await _documentSequenceQueryRepository.GenerateDocumentNumber(typeId.Value);
            var stoNumber = sequences.Count > 0 ? sequences[^1] : null;
            entity.StoNumber = stoNumber
                ?? throw new ExceptionRules("No document sequence configured for Stock Transfer Order.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "STO_HEADER_CREATE",
                actionName: stoNumber,
                details: $"Stock Transfer Order '{stoNumber}' created successfully with Id {newId}.",
                module: "StoHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Stock Transfer Order created successfully.",
                Data = newId
            };
        }
    }
}
