using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateMaster
{
    public class UpdateMasterCommandHandler : IRequestHandler<UpdateMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateMasterCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IScheduleIIIQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ScheduleIIIMaster>(request);

            var result = await _commandRepository.UpdateMasterAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "S3_MASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Schedule III master with Id {request.Id} updated successfully.",
                module: "ScheduleIIIMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Schedule III master updated successfully.",
                Data = result
            };
        }
    }
}
