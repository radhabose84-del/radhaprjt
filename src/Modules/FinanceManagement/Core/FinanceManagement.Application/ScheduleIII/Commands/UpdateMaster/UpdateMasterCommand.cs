using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateMaster
{
    public class UpdateMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int StatusId { get; set; }   // MiscMaster (S3_STATUS)
        public int TextileSplitEnabled { get; set; } // 0/1
        public int DisplayOrder { get; set; }
        public int IsActive { get; set; }            // 1 = Active, 0 = Inactive
        // CompanyId / DivisionId / ScheduleIIISectionItemId are immutable — excluded.
    }
}
