using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CountGroup.Commands.UpdateCountGroup
{
    public class UpdateCountGroupCommandHandler : IRequestHandler<UpdateCountGroupCommand, ApiResponseDTO<int>>
    {
        private readonly ICountGroupCommandRepository _commandRepository;
        private readonly ICountGroupQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateCountGroupCommandHandler(
            ICountGroupCommandRepository commandRepository,
            ICountGroupQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateCountGroupCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.CountGroup>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "COUNTGROUP_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Count Group with Id {request.Id} updated successfully.",
                module: "CountGroup"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Count Group updated successfully.",
                Data = result
            };
        }
    }
}
