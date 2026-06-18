using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateSubTotal
{
    public class UpdateSubTotalCommandHandler : IRequestHandler<UpdateSubTotalCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSubTotalCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSubTotalCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<ScheduleIIISubTotal>(request);

            var result = await _commandRepository.UpdateSubTotalAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "S3_SUBTOTAL_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Schedule III sub-total with Id {request.Id} updated successfully.",
                module: "ScheduleIIISubTotal"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sub-total updated successfully.",
                Data = result
            };
        }
    }
}
