using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateMaster
{
    // Adds one included line to the company/division structure (the merged master = one row per line).
    public class CreateMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        // CompanyId + DivisionId from the token; StatusId is auto-set to DRAFT on create (not in the payload).
        public int TextileSplitEnabled { get; set; }    // 0/1 — header
        public int ScheduleIIISectionItemId { get; set; }  // the included line
        public int DisplayOrder { get; set; }
    }
}
