using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnTwistMaster.Commands.UpdateYarnTwistMaster
{
    public class UpdateYarnTwistMasterCommandHandler : IRequestHandler<UpdateYarnTwistMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IYarnTwistMasterCommandRepository _commandRepository;
        private readonly IYarnTwistMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateYarnTwistMasterCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateYarnTwistMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.YarnTwistMaster>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "YARNTWISTMASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Yarn Twist Master with Id {request.Id} updated successfully.",
                module: "YarnTwistMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Yarn Twist Master updated successfully.",
                Data = result
            };
        }
    }
}
