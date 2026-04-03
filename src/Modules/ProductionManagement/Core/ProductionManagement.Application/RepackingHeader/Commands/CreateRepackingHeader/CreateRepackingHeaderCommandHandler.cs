using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingHeader.Commands.CreateRepackingHeader
{
    public class CreateRepackingHeaderCommandHandler
        : IRequestHandler<CreateRepackingHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IRepackingHeaderCommandRepository _commandRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public CreateRepackingHeaderCommandHandler(
            IRepackingHeaderCommandRepository commandRepository,
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
            CreateRepackingHeaderCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.RepackingHeader>(request);
            entity.UnitId = _ipAddressService.GetUnitId() ?? 0;
            entity.ProductionYear = request.RepackDate.Year;

            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // Determine transaction type dynamically: Repacking vs YarnConversion
            bool isRepacking = request.ItemId == request.OldItemId;
            var transactionTypeCode = isRepacking
                ? MiscEnumEntity.TransactionTypeRePackMaster
                : MiscEnumEntity.TransactionTypeYarnConversion;

            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                transactionTypeCode, MiscEnumEntity.ModuleSales, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules($"Transaction Type '{transactionTypeCode}' not found for Sales module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var repackDocNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.RepackDocNo = repackDocNo
                ?? throw new ExceptionRules($"No document sequence configured for {transactionTypeCode}.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            var actionCode = isRepacking ? "REPACKING_CREATE" : "YARN_CONVERSION_CREATE";
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: actionCode,
                actionName: entity.RepackDocNo,
                details: $"RepackingHeader '{entity.RepackDocNo}' created successfully with Id {newId}.",
                module: "RepackingHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = isRepacking
                    ? "Repacking created successfully."
                    : "Yarn Conversion created successfully.",
                Data = newId
            };
        }
    }
}
