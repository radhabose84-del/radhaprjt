using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaInvoice
{
    public class UpdateProformaInvoiceCommandHandler : IRequestHandler<UpdateProformaInvoiceCommand, ApiResponseDTO<int>>
    {
        private readonly IProformaInvoiceCommandRepository _commandRepository;
        private readonly IProformaInvoiceQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateProformaInvoiceCommandHandler(
            IProformaInvoiceCommandRepository commandRepository,
            IProformaInvoiceQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateProformaInvoiceCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ProformaInvoice>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "PROFORMA_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Proforma Invoice with Id {request.Id} updated successfully.",
                module: "ProformaInvoice");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Proforma Invoice updated successfully.",
                Data = result
            };
        }
    }
}
