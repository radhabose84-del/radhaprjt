using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.LotMaster.Commands.CreateLotMaster
{
    public class CreateLotMasterCommandHandler : IRequestHandler<CreateLotMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ILotMasterCommandRepository _commandRepository;
        private readonly ILotMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateLotMasterCommandHandler(
            ILotMasterCommandRepository commandRepository,
            ILotMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateLotMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.LotMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "LOTMASTER_CREATE",
                actionName: request.LotCode ?? string.Empty,
                details: $"LotMaster '{request.LotCode}' created successfully with Id {newId}.",
                module: "LotMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Lot Master created successfully.",
                Data = newId
            };
        }
    }
}
