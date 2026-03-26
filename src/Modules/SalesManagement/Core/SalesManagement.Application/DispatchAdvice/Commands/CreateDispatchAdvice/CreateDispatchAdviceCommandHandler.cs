using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Commands.CreateDispatchAdvice
{
    public class CreateDispatchAdviceCommandHandler : IRequestHandler<CreateDispatchAdviceCommand, ApiResponseDTO<int>>
    {
        private readonly IDispatchAdviceCommandRepository _commandRepository;
        private readonly IDispatchAdviceQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateDispatchAdviceCommandHandler(
            IDispatchAdviceCommandRepository commandRepository,
            IDispatchAdviceQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateDispatchAdviceCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<DispatchAdviceHeader>(request);

            // Set Draft status
            var draftStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Pending);
            entity.StatusId = draftStatus?.Id ?? 0;

            // Get UnitId from JWT or SalesOrder
            var unitId = _ipAddressService.GetUnitId() ?? await _queryRepository.GetSalesOrderUnitIdAsync(request.SalesOrderId);
            entity.UnitId = unitId;

            // Generate DispatchNo from DocumentSequence
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeDispatchAdvice, MiscEnumEntity.ModuleSales, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Dispatch Advice' not found for Sales module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var dispatchNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.DispatchNo = dispatchNo
                ?? throw new ExceptionRules("No document sequence configured for Dispatch Advice.");

            // Resolve Packed and Reserved status IDs for StockLedger update
            var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Packed);
            var packedStatusId = packedStatus?.Id ?? 0;

            var reservedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Reserved);
            var reservedStatusId = reservedStatus?.Id ?? 0;

            // CreateAsync inserts header + details, updates StockLedger, increments DocNo
            var newId = await _commandRepository.CreateAsync(entity, unitId, packedStatusId, reservedStatusId, typeId.Value);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "DISPATCHADVICE_CREATE",
                actionName: dispatchNo,
                details: $"Dispatch Advice '{dispatchNo}' created successfully with Id {newId}.",
                module: "DispatchAdvice");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Dispatch Advice created successfully.",
                Data = newId
            };
        }
    }
}
