using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProcessMaster.Commands.UpdateProcessMaster
{
    public class UpdateProcessMasterCommandHandler : IRequestHandler<UpdateProcessMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IProcessMasterCommandRepository _commandRepository;
        private readonly IProcessMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateProcessMasterCommandHandler(
            IProcessMasterCommandRepository commandRepository,
            IProcessMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateProcessMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ProcessMaster>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "PROCESSMASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Process Master with Id {request.Id} updated successfully.",
                module: "ProcessMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Process Master updated successfully.",
                Data = result
            };
        }
    }
}
