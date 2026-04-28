using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation
{
    public class CreateSalesQuotationCommandHandler : IRequestHandler<CreateSalesQuotationCommand, int>
    {
        private readonly ISalesQuotationCommandRepository _commandRepository;
        private readonly ISalesQuotationQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;

        public CreateSalesQuotationCommandHandler(
            ISalesQuotationCommandRepository commandRepository,
            ISalesQuotationQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<int> Handle(CreateSalesQuotationCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<SalesQuotationHeader>(request);

            // Set default StatusId to 'Pending'
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.InvoiceApprovalStatus, MiscEnumEntity.InvoiceStatusPending);
            entity.StatusId = pendingStatus?.Id;

            // Generate QuotationNo from DocumentSequence
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeSalesQuotation,
                MiscEnumEntity.ModuleSales,
                unitId);

            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Sales Quotation' not found. Please configure it in Transaction Type Master.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var quotationNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.QuotationNo = quotationNo
                ?? throw new ExceptionRules("No document sequence configured for Sales Quotation.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALESQUOTATION_CREATE",
                actionName: quotationNo,
                details: $"Sales Quotation '{quotationNo}' created successfully with Id {newId}.",
                module: "SalesQuotation");
            await _mediator.Publish(auditEvent, cancellationToken);

            return newId > 0 ? newId : throw new ExceptionRules("Sales Quotation Creation Failed.");
        }
    }
}
