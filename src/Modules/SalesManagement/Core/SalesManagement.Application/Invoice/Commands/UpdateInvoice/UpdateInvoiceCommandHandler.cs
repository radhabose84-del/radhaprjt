using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Commands.UpdateInvoice
{
    public class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand, ApiResponseDTO<int>>
    {
        private readonly IInvoiceCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateInvoiceCommandHandler(
            IInvoiceCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<InvoiceHeader>(request);

            // Get UnitId from JWT token
            var unitId = _ipAddressService.GetUnitId();

            // Resolve StockLedger status IDs from MiscMaster
            var dispatchedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Reserved);
            var dispatchedStatusId = dispatchedStatus?.Id ?? 0;

            var invoicedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Invoiced);
            var invoicedStatusId = invoicedStatus?.Id ?? 0;

            var result = await _commandRepository.UpdateAsync(entity, unitId, dispatchedStatusId, invoicedStatusId);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "INVOICE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Invoice with Id {request.Id} updated successfully.",
                module: "Invoice");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Invoice updated successfully.",
                Data = result
            };
        }
    }
}
