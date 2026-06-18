using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.LockStructure
{
    public class LockStructureCommandHandler : IRequestHandler<LockStructureCommand, ApiResponseDTO<bool>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public LockStructureCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IMediator mediator,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<bool>> Handle(LockStructureCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var divisionId = _ipAddressService.GetDivisionId()
                ?? throw new ExceptionRules("No active division in session.");

            var result = await _commandRepository.LockStructureAsync(companyId, divisionId);

            if (!result)
                throw new ExceptionRules("Structure not found or 'Locked' status is not configured.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Lock",
                actionCode: "S3_STRUCTURE_LOCK",
                actionName: $"{companyId}/{divisionId}",
                details: $"Schedule III structure for company {companyId}, division {divisionId} locked.",
                module: "ScheduleIIIHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<bool>
            {
                IsSuccess = true,
                Message = "Structure locked successfully.",
                Data = result
            };
        }
    }
}
