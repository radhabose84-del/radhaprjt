using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeRateVersion
{
    public class CreateTaxCodeRateVersionCommandHandler : IRequestHandler<CreateTaxCodeRateVersionCommand, ApiResponseDTO<int>>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public CreateTaxCodeRateVersionCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateTaxCodeRateVersionCommand request, CancellationToken cancellationToken)
        {
            var newVersionId = await _commandRepository.CreateRateVersionAsync(new Domain.Entities.TaxCodeRateVersion
            {
                TaxCodeId = request.TaxCodeId,
                RatePercent = request.RatePercent,
                EffectiveFrom = request.EffectiveFrom,
                ChangeReason = request.ChangeReason
            });

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "TAX_CODE_RATE_VERSION_CREATE",
                actionName: request.TaxCodeId.ToString(),
                details: $"New rate version created for Tax Code Id {request.TaxCodeId}, effective {request.EffectiveFrom:yyyy-MM-dd}.",
                module: "TaxCodeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Tax code rate version created successfully.",
                Data = newVersionId
            };
        }
    }
}
