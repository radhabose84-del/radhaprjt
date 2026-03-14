using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster
{
    public class UpdateMiscTypeMasterCommandHandler : IRequestHandler<UpdateMiscTypeMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IMiscTypeMasterCommandRepository _commandRepository;
        private readonly IMiscTypeMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateMiscTypeMasterCommandHandler(
            IMiscTypeMasterCommandRepository commandRepository,
            IMiscTypeMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateMiscTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.MiscTypeMaster>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "MISC_TYPE_MASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Misc Type Master with Id {request.Id} updated successfully.",
                module: "MiscTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Misc Type Master updated successfully.",
                Data = result
            };
        }
    }
}
