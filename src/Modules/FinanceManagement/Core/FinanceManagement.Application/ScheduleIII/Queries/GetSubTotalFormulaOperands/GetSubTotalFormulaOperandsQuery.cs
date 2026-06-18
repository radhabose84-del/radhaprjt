using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetSubTotalFormulaOperands
{
    // Operand picker for the "Edit formula" dialog: active P&L line items + S3_SUBTOTAL_TYPE nodes.
    public class GetSubTotalFormulaOperandsQuery : IRequest<ApiResponseDTO<List<SubTotalFormulaOperandDto>>>
    {
        // When editing an existing sub-total, supply its id to get the current +/− selection per operand.
        public int? SubTotalId { get; set; }
    }
}
