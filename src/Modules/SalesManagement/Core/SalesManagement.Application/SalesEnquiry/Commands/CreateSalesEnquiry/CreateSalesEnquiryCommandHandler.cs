using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry
{
    public class CreateSalesEnquiryCommandHandler : IRequestHandler<CreateSalesEnquiryCommand, int>
    {
        private readonly ISalesEnquiryCommandRepository _commandRepository;
        private readonly ISalesEnquiryQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateSalesEnquiryCommandHandler(
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

        public async Task<int> Handle(CreateSalesEnquiryCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<SalesEnquiryHeader>(request.SalesEnquiryDetails);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALESENQUIRY_CREATE",
                actionName: newId.ToString(),
                details: $"Sales Enquiry created successfully with Id {newId}.",
                module: "SalesEnquiry");
            await _mediator.Publish(auditEvent, cancellationToken);

            return newId > 0 ? newId : throw new ExceptionRules("Sales Enquiry Creation Failed.");
        }
    }
}
