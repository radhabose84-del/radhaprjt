using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Commands.UpdateInvoice
{
    public class UpdateInvoiceCommandHandler : IRequestHandler<UpdateInvoiceCommand, ApiResponseDTO<int>>
    {
        private readonly IInvoiceCommandRepository _commandRepository;
        private readonly IInvoiceQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateInvoiceCommandHandler(
            IInvoiceCommandRepository commandRepository,
            IInvoiceQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<InvoiceHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

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
