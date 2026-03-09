using AutoMapper;
using Contracts.Common;
using MediatR;
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
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateInvoiceCommandHandler(
            IInvoiceCommandRepository commandRepository,
            IInvoiceQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<InvoiceHeader>(request);

            // Generate auto invoice number
            var invoiceNo = await _commandRepository.GenerateNextInvoiceNoAsync(request.UnitId, cancellationToken);
            entity.InvoiceNo = invoiceNo;

            // Resolve StockLedger status IDs from MiscMaster
            var dispatchedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Dispatched);
            var dispatchedStatusId = dispatchedStatus?.Id ?? 0;

            var invoicedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Invoiced);
            var invoicedStatusId = invoicedStatus?.Id ?? 0;

            // Insert header + details, update StockLedger Dispatched → Invoiced
            var newId = await _commandRepository.CreateAsync(entity, request.UnitId, dispatchedStatusId, invoicedStatusId);

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
