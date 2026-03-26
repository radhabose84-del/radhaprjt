using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnType.Commands.UpdateYarnType
{
    public class UpdateYarnTypeCommandHandler : IRequestHandler<UpdateYarnTypeCommand, ApiResponseDTO<int>>
    {
        private readonly IYarnTypeCommandRepository _commandRepository;
        private readonly IYarnTypeQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateYarnTypeCommandHandler(
            IYarnTypeCommandRepository commandRepository,
            IYarnTypeQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateYarnTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.YarnType>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "YARNTYPE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Yarn Type with Id {request.Id} updated successfully.",
                module: "YarnType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Yarn Type updated successfully.",
                Data = result
            };
        }
    }
}
