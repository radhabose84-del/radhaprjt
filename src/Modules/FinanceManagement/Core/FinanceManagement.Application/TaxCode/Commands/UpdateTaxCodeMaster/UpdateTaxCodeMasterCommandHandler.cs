using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.UpdateTaxCodeMaster
{
    public class UpdateTaxCodeMasterCommandHandler : IRequestHandler<UpdateTaxCodeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateTaxCodeMasterCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateTaxCodeMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.TaxCodeMaster>(request);

            var updatedId = await _commandRepository.UpdateTaxCodeAsync(entity);

            // Merged rate-version flow: a supplied rate creates a new effective-dated version
            // (closes the prior open one). Never updated in place (AC3-A).
            if (request.RatePercent.HasValue && request.RateEffectiveFrom.HasValue)
            {
                await _commandRepository.CreateRateVersionAsync(new Domain.Entities.TaxCodeRateVersion
                {
                    TaxCodeId = request.Id,
                    RatePercent = request.RatePercent.Value,
                    EffectiveFrom = request.RateEffectiveFrom.Value,
                    ChangeReason = request.RateChangeReason
                });
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "TAX_CODE_MASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Tax Code with Id {request.Id} updated successfully.",
                module: "TaxCodeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Tax Code updated successfully.",
                Data = updatedId
            };
        }
    }
}
