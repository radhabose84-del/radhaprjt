using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Domain.Entities;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.Repacking.Commands.UpdateRepacking
{
    public class UpdateRepackingCommandHandler : IRequestHandler<UpdateRepackingCommand, ApiResponseDTO<int>>
    {
        private readonly IRepackingCommandRepository _commandRepository;
        private readonly IRepackingQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateRepackingCommandHandler(
            IRepackingCommandRepository commandRepository,
            IRepackingQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateRepackingCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<RepackingHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "REPACKING_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Repacking with Id {request.Id} updated successfully.",
                module: "Production"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Repacking updated successfully.",
                Data = result
            };
        }
    }
}
