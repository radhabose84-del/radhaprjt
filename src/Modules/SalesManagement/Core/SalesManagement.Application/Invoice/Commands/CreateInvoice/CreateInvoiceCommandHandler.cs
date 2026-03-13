using AutoMapper;
using Contracts.Common;
using MediatR;
using Contracts.Interfaces;
using SalesManagement.Application.Common.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Commands.CreateInvoice
{
    public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, ApiResponseDTO<int>>
    {
        private readonly IInvoiceCommandRepository _commandRepository;
        private readonly IInvoiceQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateInvoiceCommandHandler(
            IInvoiceCommandRepository commandRepository,
            IInvoiceQueryRepository queryRepository,
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

        public async Task<ApiResponseDTO<int>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<InvoiceHeader>(request);

            // Set default StatusId to 'Pending'
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.InvoiceApprovalStatus, MiscEnumEntity.InvoiceStatusPending);
            entity.StatusId = pendingStatus?.Id;

            // Get UnitId from JWT token
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // Generate invoice number from DocumentSequence
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeInvoice, MiscEnumEntity.ModuleSales, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Invoice' not found for Sales module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var invoiceNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.InvoiceNo = invoiceNo
                ?? throw new ExceptionRules("No document sequence configured for Invoice.");

            // Resolve StockLedger status IDs from MiscMaster
            var dispatchedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Reserved);
            var dispatchedStatusId = dispatchedStatus?.Id ?? 0;

            var invoicedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Invoiced);
            var invoicedStatusId = invoicedStatus?.Id ?? 0;

            // Insert header + details, update StockLedger Dispatched → Invoiced, increment DocNo
            var newId = await _commandRepository.CreateAsync(entity, unitId, dispatchedStatusId, invoicedStatusId, typeId.Value);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "INVOICE_CREATE",
                actionName: invoiceNo,
                details: $"Invoice '{invoiceNo}' created successfully with Id {newId}.",
                module: "Invoice");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Invoice created successfully.",
                Data = newId
            };
        }
    }
}
