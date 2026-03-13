using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CountMaster.Commands.UpdateCountMaster
{
    public class UpdateCountMasterCommandHandler : IRequestHandler<UpdateCountMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ICountMasterCommandRepository _commandRepository;
        private readonly ICountMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateCountMasterCommandHandler(
            ICountMasterCommandRepository commandRepository,
            ICountMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateCountMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.CountMaster>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "COUNT_MASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Count Master with Id {request.Id} updated successfully.",
                module: "CountMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Count Master updated successfully.",
                Data = result
            };
        }
    }
}
