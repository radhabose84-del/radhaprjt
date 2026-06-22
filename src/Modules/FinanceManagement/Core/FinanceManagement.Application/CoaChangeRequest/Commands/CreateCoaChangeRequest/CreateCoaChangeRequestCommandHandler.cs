using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaChangeRequest
{
    public class CreateCoaChangeRequestCommandHandler : IRequestHandler<CreateCoaChangeRequestCommand, ApiResponseDTO<int>>
    {
        private readonly ICoaChangeRequestCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;

        public CreateCoaChangeRequestCommandHandler(
            ICoaChangeRequestCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateCoaChangeRequestCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var userId = _ipAddressService.GetUserId();
            var now = _timeZoneService.GetCurrentTime();

            var entity = new Domain.Entities.CoaChangeRequest
            {
                CompanyId = companyId,
                TargetAccountId = request.TargetAccountId,
                TargetAccountGroupId = request.TargetAccountGroupId,
                AccountCodeSnapshot = request.AccountCodeSnapshot,
                ChangeType = request.ChangeType,
                Justification = request.Justification,
                ImpactAssessment = request.ImpactAssessment,
                RequestStatus = CoaChangeRequestStatus.PendingImpactApproval,
                RequestedByUserId = userId,
                RequestedOn = now
            };

            await _commandRepository.AddChangeRequestWithoutSaveAsync(entity, cancellationToken);
            await _commandRepository.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COA_CHANGE_REQUEST_CREATE",
                actionName: entity.Id.ToString(),
                details: $"COA change request {entity.Id} ({request.ChangeType}) raised with impact assessment; pending CFO impact approval.",
                module: "CoaChangeRequest"), cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Change request raised. Awaiting CFO impact approval.",
                Data = entity.Id
            };
        }
    }
}
