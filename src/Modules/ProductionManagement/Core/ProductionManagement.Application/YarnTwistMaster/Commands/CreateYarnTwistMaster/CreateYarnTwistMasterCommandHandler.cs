using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnTwistMaster.Commands.CreateYarnTwistMaster
{
    public class CreateYarnTwistMasterCommandHandler : IRequestHandler<CreateYarnTwistMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IYarnTwistMasterCommandRepository _commandRepository;
        private readonly IYarnTwistMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateYarnTwistMasterCommandHandler(
            IYarnTwistMasterCommandRepository commandRepository,
            IYarnTwistMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateYarnTwistMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.YarnTwistMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "YARNTWISTMASTER_CREATE",
                actionName: request.TwistName ?? string.Empty,
                details: $"Yarn Twist Master '{request.TwistName}' created successfully with Id {newId}.",
                module: "YarnTwistMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Yarn Twist Master created successfully.",
                Data = newId
            };
        }
    }
}
