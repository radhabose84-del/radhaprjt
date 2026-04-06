using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnConversionHeader.Commands.CreateYarnConversionHeader
{
    public class CreateYarnConversionHeaderCommandHandler : IRequestHandler<CreateYarnConversionHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IYarnConversionHeaderCommandRepository _commandRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public CreateYarnConversionHeaderCommandHandler(
            IYarnConversionHeaderCommandRepository commandRepository,
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

        public async Task<ApiResponseDTO<int>> Handle(CreateYarnConversionHeaderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.YarnConversionHeader>(request);

            var unitId = _ipAddressService.GetUnitId() ?? 0;
            entity.UnitId = unitId;

            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeYarnConversion,
                MiscEnumEntity.ModuleSales,
                unitId);

            if (typeId == null)
                throw new ExceptionRules("Transaction Type not found for Yarn Conversion.");

            var docNumbers = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            if (docNumbers == null || docNumbers.Count == 0)
                throw new ExceptionRules("No document sequence configured for Yarn Conversion.");

            entity.ConversionDocNo = docNumbers[0];
            entity.TypeId = typeId.Value;

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "YARN_CONVERSION_CREATE",
                actionName: entity.ConversionDocNo ?? string.Empty,
                details: $"Yarn Conversion '{entity.ConversionDocNo}' created successfully with Id {newId}.",
                module: "YarnConversionHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Yarn Conversion created successfully.",
                Data = newId
            };
        }
    }
}
