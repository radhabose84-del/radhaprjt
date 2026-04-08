using AutoMapper;
using Contracts.Common;
using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Domain.Events;

namespace LogisticsManagement.Application.FreightMaster.Commands.CreateFreightMaster
{
    public class CreateFreightMasterCommandHandler : IRequestHandler<CreateFreightMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IFreightMasterCommandRepository _commandRepository;
        private readonly IFreightMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateFreightMasterCommandHandler(
            IFreightMasterCommandRepository commandRepository,
            IFreightMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateFreightMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.FreightMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "FREIGHT_MASTER_CREATE",
                actionName: newId.ToString(),
                details: $"FreightMaster created successfully with Id {newId}.",
                module: "FreightMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "FreightMaster created successfully.",
                Data = newId
            };
        }
    }
}
