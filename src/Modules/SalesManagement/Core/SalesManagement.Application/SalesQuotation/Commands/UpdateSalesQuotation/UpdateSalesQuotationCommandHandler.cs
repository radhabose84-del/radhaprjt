using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesQuotation.Commands.UpdateSalesQuotation
{
    public class UpdateSalesQuotationCommandHandler : IRequestHandler<UpdateSalesQuotationCommand, int>
    {
        private readonly ISalesQuotationCommandRepository _commandRepository;
        private readonly ISalesQuotationQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdateSalesQuotationCommandHandler(
            ISalesQuotationCommandRepository commandRepository,
            ISalesQuotationQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<int> Handle(UpdateSalesQuotationCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<SalesQuotationHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALESQUOTATION_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Quotation with Id {request.Id} updated successfully.",
                module: "SalesQuotation");
            await _mediator.Publish(auditEvent, cancellationToken);

            return result;
        }
    }
}
