using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IPackType;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.PackType.Commands.UpdatePackType
{
    public class UpdatePackTypeCommandHandler : IRequestHandler<UpdatePackTypeCommand, ApiResponseDTO<int>>
    {
        private readonly IPackTypeCommandRepository _commandRepository;
        private readonly IPackTypeQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdatePackTypeCommandHandler(
            IPackTypeCommandRepository commandRepository,
            IPackTypeQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdatePackTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.PackType>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "PACKTYPE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"PackType with Id {request.Id} updated successfully.",
                module: "PackType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "PackType updated successfully.",
                Data = result
            };
        }
    }
}
