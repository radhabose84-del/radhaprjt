using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ProformaInvoice.Commands.CreateProformaInvoice
{
    public class CreateProformaInvoiceCommandHandler : IRequestHandler<CreateProformaInvoiceCommand, ApiResponseDTO<int>>
    {
        private readonly IProformaInvoiceCommandRepository _commandRepository;
        private readonly IProformaInvoiceQueryRepository _queryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateProformaInvoiceCommandHandler(
            IProformaInvoiceCommandRepository commandRepository,
            IProformaInvoiceQueryRepository queryRepository,
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

        public async Task<ApiResponseDTO<int>> Handle(CreateProformaInvoiceCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ProformaInvoice>(request);

            // Calculate SOBalance = remaining SO balance AFTER this proforma
            var soBalance = await _queryRepository.GetSalesOrderBalanceAsync(request.SalesOrderId);
            entity.SOBalance = soBalance - request.ProformaAmount;

            // Generate ProformaNumber via IDocumentSequenceLookup
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeProformaInvoice, MiscEnumEntity.ModuleSales, unitId);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Proforma Invoice' not found for Sales module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var proformaNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.ProformaNumber = proformaNo
                ?? throw new ExceptionRules("No document sequence configured for Proforma Invoice.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "PROFORMA_CREATE",
                actionName: proformaNo,
                details: $"Proforma Invoice '{proformaNo}' created successfully with Id {newId}.",
                module: "ProformaInvoice");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Proforma Invoice created successfully.",
                Data = newId
            };
        }
    }
}
