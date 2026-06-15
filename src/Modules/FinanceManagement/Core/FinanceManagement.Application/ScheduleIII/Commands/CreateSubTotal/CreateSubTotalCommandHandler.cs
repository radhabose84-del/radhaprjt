using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal
{
    public class CreateSubTotalCommandHandler : IRequestHandler<CreateSubTotalCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSubTotalCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSubTotalCommand request, CancellationToken cancellationToken)
        {
            var subTotal = _mapper.Map<ScheduleIIISubTotal>(request);

            var formulas = request.Formulas.Select(f => new ScheduleIIISubTotalFormula
            {
                OperandTypeId = f.OperandTypeId,
                OperandRefId = f.OperandRefId,
                OperatorId = f.OperatorId,
                DisplayOrder = f.DisplayOrder
            }).ToList();

            var newId = await _commandRepository.CreateSubTotalAsync(subTotal, formulas);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "S3_SUBTOTAL_CREATE",
                actionName: request.SubTotalName ?? string.Empty,
                details: $"Schedule III sub-total '{request.SubTotalName}' created successfully with Id {newId}.",
                module: "ScheduleIIISubTotal"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sub-total created successfully.",
                Data = newId
            };
        }
    }
}
