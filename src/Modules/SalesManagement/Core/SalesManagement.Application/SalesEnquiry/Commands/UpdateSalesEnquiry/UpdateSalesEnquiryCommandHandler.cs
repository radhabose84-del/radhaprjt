using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesEnquiry.Commands.UpdateSalesEnquiry
{
    public class UpdateSalesEnquiryCommandHandler : IRequestHandler<UpdateSalesEnquiryCommand, int>
    {
        private readonly ISalesEnquiryCommandRepository _commandRepository;
        private readonly ISalesEnquiryQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdateSalesEnquiryCommandHandler(
            ISalesEnquiryCommandRepository commandRepository,
            ISalesEnquiryQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<int> Handle(UpdateSalesEnquiryCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<SalesEnquiryHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALESENQUIRY_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Enquiry with Id {request.Id} updated successfully.",
                module: "SalesEnquiry");
            await _mediator.Publish(auditEvent, cancellationToken);

            return result;
        }
    }
}
