using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.MixCodeMaster.Commands.UpdateMixCodeMaster
{
    public class UpdateMixCodeMasterCommandHandler : IRequestHandler<UpdateMixCodeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IMixCodeMasterCommandRepository _commandRepository;
        private readonly IMixCodeMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateMixCodeMasterCommandHandler(
            IMixCodeMasterCommandRepository commandRepository,
            IMixCodeMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateMixCodeMasterCommand request, CancellationToken cancellationToken)
        {
            // Rule #25 — block inactivation while linked to an ArrivalDetail
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsMixCodeMasterLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.MixCodeMaster>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "MIXCODEMASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"MixCodeMaster with Id {request.Id} updated successfully.",
                module: "MixCodeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Mix Code updated successfully.",
                Data = result
            };
        }
    }
}
