using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesLead.Commands.CreateSalesLead
{
    public class CreateSalesLeadCommandHandler : IRequestHandler<CreateSalesLeadCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesLeadCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSalesLeadCommandHandler(
            ISalesLeadCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesLeadCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesLead>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALES_LEAD_CREATE",
                actionName: request.ContactName ?? request.MarketingPersonId.ToString(),
                details: $"Sales Lead created successfully with Id {newId}.",
                module: "SalesLead"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Lead created successfully.",
                Data = newId
            };
        }
    }
}
