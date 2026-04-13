using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RawMaterialType.Commands.UpdateRawMaterialType
{
    public class UpdateRawMaterialTypeCommandHandler : IRequestHandler<UpdateRawMaterialTypeCommand, ApiResponseDTO<int>>
    {
        private readonly IRawMaterialTypeCommandRepository _commandRepository;
        private readonly IRawMaterialTypeQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateRawMaterialTypeCommandHandler(
            IRawMaterialTypeCommandRepository commandRepository,
            IRawMaterialTypeQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateRawMaterialTypeCommand request, CancellationToken cancellationToken)
        {
            // Story §3 — "Deactivate: Allow deactivation with warning if in use".
            // Today no dependent entity references RawMaterialTypeId, so the linked check
            // always returns false. The hook is wired up so that when a future module adds
            // a FK to RawMaterialType, the only change required is to make
            // IsRawMaterialTypeLinkedAsync return true on real dependents (Rule #25).
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsRawMaterialTypeLinkedAsync(request.Id);
                if (isLinked)
                {
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
                }
            }

            var entity = _mapper.Map<Domain.Entities.RawMaterialType>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "RAWMATERIALTYPE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Raw Material Type with Id {request.Id} updated successfully.",
                module: "RawMaterialType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Raw Material Type updated successfully.",
                Data = result
            };
        }
    }
}
