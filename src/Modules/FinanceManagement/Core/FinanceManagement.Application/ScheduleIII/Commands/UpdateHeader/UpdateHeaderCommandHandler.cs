using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateHeader
{
    public class UpdateHeaderCommandHandler : IRequestHandler<UpdateHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public UpdateHeaderCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IMediator mediator,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateHeaderCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var divisionId = _ipAddressService.GetDivisionId()
                ?? throw new ExceptionRules("No active division in session.");

            var result = await _commandRepository.UpdateHeaderAsync(
                companyId, divisionId, request.StatusId, request.TextileSplitEnabled == 1);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "S3_HEADER_UPDATE",
                actionName: $"{companyId}/{divisionId}",
                details: $"Schedule III header for company {companyId}, division {divisionId} updated successfully.",
                module: "ScheduleIIIHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Schedule III header updated successfully.",
                Data = result
            };
        }
    }
}
