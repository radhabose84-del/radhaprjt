using AutoMapper;
using Contracts.Common;
using Contracts.Events.Coa;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaRead;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.Common.Interfaces.IIntegrationEvents;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.UpdateGlAccountMaster
{
    public class UpdateGlAccountMasterCommandHandler : IRequestHandler<UpdateGlAccountMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IGlAccountMasterCommandRepository _commandRepository;
        private readonly IGlAccountMasterQueryRepository _queryRepository;
        private readonly IGlobalCoaPropagationService _propagationService;
        private readonly ICoaReadQueryRepository _coaReadRepository;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateGlAccountMasterCommandHandler(
            IGlAccountMasterCommandRepository commandRepository,
            IGlAccountMasterQueryRepository queryRepository,
            IGlobalCoaPropagationService propagationService,
            ICoaReadQueryRepository coaReadRepository,
            IIntegrationEventPublisher integrationEventPublisher,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _propagationService = propagationService;
            _coaReadRepository = coaReadRepository;
            _integrationEventPublisher = integrationEventPublisher;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateGlAccountMasterCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            // US-GL02-16 (AC3) — capture the prior state so we only fire the event on a real
            // active -> inactive transition.
            var before = await _coaReadRepository.GetPostingInfoByIdAsync(companyId, request.Id, cancellationToken);

            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsGlAccountLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This GL Account is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.GlAccountMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            // US-GL02-10 (AC3) — if this is a global template account, push the edit to its per-company
            // copies (skipping any that carry a local override). No-op for non-global accounts.
            await _propagationService.PropagateUpdateAsync(request.Id, cancellationToken);

            // US-GL02-16 (AC3) — account went active -> inactive: publish the deactivation event within 1s
            // (direct bus publish, outbox fallback) so AP/AR/FA can react.
            if (before is { IsActive: true } && request.IsActive == 0)
            {
                var correlationId = Guid.NewGuid();
                await _integrationEventPublisher.PublishWithinSlaAsync(new GlAccountDeactivatedEvent
                {
                    CorrelationId = correlationId,
                    CompanyId = companyId,
                    AccountId = request.Id,
                    AccountCode = before.AccountCode,
                    AccountName = before.AccountName,
                    DeactivatedBy = _ipAddressService.GetUserId(),
                    DeactivatedAt = DateTimeOffset.UtcNow
                }, correlationId, cancellationToken);
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "GL_ACCOUNT_MASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"GL Account with Id {request.Id} updated successfully.",
                module: "GlAccountMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "GL Account updated successfully.",
                Data = updatedId
            };
        }
    }
}
