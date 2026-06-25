using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.InitializeSubsidiaryCoa
{
    public class InitializeSubsidiaryCoaCommandHandler : IRequestHandler<InitializeSubsidiaryCoaCommand, ApiResponseDTO<int>>
    {
        private readonly IGlobalCoaPropagationService _propagationService;
        private readonly IMediator _mediator;

        public InitializeSubsidiaryCoaCommandHandler(
            IGlobalCoaPropagationService propagationService,
            IMediator mediator)
        {
            _propagationService = propagationService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(InitializeSubsidiaryCoaCommand request, CancellationToken cancellationToken)
        {
            var created = await _propagationService.InheritForCompanyAsync(request.CompanyId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COA_SUBSIDIARY_INHERIT",
                actionName: request.CompanyId.ToString(),
                details: $"Subsidiary {request.CompanyId} inherited {created} global COA account(s).",
                module: "GlAccountMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = $"{created} global account(s) inherited for the subsidiary.",
                Data = created
            };
        }
    }
}
