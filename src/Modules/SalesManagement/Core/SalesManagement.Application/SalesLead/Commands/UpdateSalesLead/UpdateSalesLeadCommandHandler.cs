using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesLead.Commands.UpdateSalesLead
{
    public class UpdateSalesLeadCommandHandler : IRequestHandler<UpdateSalesLeadCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesLeadCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSalesLeadCommandHandler(
            ISalesLeadCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesLeadCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesLead>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALES_LEAD_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Lead with Id {request.Id} updated successfully.",
                module: "SalesLead"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Lead updated successfully.",
                Data = updatedId
            };
        }
    }
}
