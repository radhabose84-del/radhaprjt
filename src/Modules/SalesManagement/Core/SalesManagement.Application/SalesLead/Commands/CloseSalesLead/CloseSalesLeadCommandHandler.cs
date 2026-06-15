using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesLead.Commands.CloseSalesLead
{
    public class CloseSalesLeadCommandHandler : IRequestHandler<CloseSalesLeadCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesLeadCommandRepository _commandRepository;
        private readonly ISalesLeadQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ITimeZoneService _timeZoneService;

        public CloseSalesLeadCommandHandler(
            ISalesLeadCommandRepository commandRepository,
            ISalesLeadQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper,
            ITimeZoneService timeZoneService)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _timeZoneService = timeZoneService;
        }

        public async Task<ApiResponseDTO<int>> Handle(CloseSalesLeadCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesLead>(request);

            // Won closures carry a conversion target and no reason; non-Won carry a reason and no target.
            var isWon = await _queryRepository.IsWonClosureTypeAsync(request.ClosureTypeId);
            if (isWon)
                entity.ClosureReasonId = null;
            else
                entity.ConvertWonLeadToId = null;

            // Closure Date is auto-populated by the server (non-editable).
            entity.ClosureDate = _timeZoneService.GetCurrentTime(_timeZoneService.GetSystemTimeZone());

            var updatedId = await _commandRepository.CloseAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Close",
                actionCode: "SALES_LEAD_CLOSE",
                actionName: request.Id.ToString(),
                details: $"Sales Lead with Id {request.Id} closed successfully.",
                module: "SalesLead"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Lead closed successfully.",
                Data = updatedId
            };
        }
    }
}
