using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.SaveSubTotalFormula
{
    public class SaveSubTotalFormulaCommandHandler : IRequestHandler<SaveSubTotalFormulaCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public SaveSubTotalFormulaCommandHandler(IScheduleIIICommandRepository commandRepository, IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(SaveSubTotalFormulaCommand request, CancellationToken cancellationToken)
        {
            var formulas = request.Formulas.Select(f => new ScheduleIIISubTotalFormula
            {
                SubTotalId = request.SubTotalId,
                OperandTypeId = f.OperandTypeId,
                SectionItemId = f.SectionItemId,
                OperandSubTotalId = f.OperandSubTotalId,
                OperatorId = f.OperatorId,
                DisplayOrder = f.DisplayOrder
            }).ToList();

            var result = await _commandRepository.SaveSubTotalFormulaAsync(request.SubTotalId, formulas);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "S3_SUBTOTAL_FORMULA_SAVE",
                actionName: request.SubTotalId.ToString(),
                details: $"Schedule III sub-total {request.SubTotalId} formula operands saved successfully.",
                module: "ScheduleIIISubTotalFormula"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sub-total formula saved successfully.",
                Data = result
            };
        }
    }
}
