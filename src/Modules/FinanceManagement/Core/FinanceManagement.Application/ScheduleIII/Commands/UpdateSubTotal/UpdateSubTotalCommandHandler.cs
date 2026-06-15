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

        public UpdateSubTotalCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSubTotalCommand request, CancellationToken cancellationToken)
        {
            var formulas = request.Formulas.Select(f => new ScheduleIIISubTotalFormula
            {
                SubTotalId = request.Id,
                OperandTypeId = f.OperandTypeId,
                OperandRefId = f.OperandRefId,
                OperatorId = f.OperatorId,
                DisplayOrder = f.DisplayOrder
            }).ToList();

            var result = await _commandRepository.UpdateSubTotalAsync(
                request.Id, request.SubTotalName, request.IncludeOtherIncome, formulas);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "S3_SUBTOTAL_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Schedule III sub-total with Id {request.Id} (formula) updated successfully.",
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
