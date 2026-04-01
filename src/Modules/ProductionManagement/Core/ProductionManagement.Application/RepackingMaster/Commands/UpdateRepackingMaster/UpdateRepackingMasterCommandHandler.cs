using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingMaster.Commands.UpdateRepackingMaster
{
    public class UpdateRepackingMasterCommandHandler
        : IRequestHandler<UpdateRepackingMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IRepackingMasterCommandRepository _commandRepository;
        private readonly IRepackingMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateRepackingMasterCommandHandler(
            IRepackingMasterCommandRepository commandRepository,
            IRepackingMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            UpdateRepackingMasterCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.RepackingMaster>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "REPACKING_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Repacking with Id {request.Id} updated successfully.",
                module: "RepackingMaster"
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
