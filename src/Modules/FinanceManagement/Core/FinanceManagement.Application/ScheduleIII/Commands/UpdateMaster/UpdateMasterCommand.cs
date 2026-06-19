using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateMaster
{
    // Updates one included line (ScheduleIIIDetail). Id is the detail row id.
    public class UpdateMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }                          // ScheduleIIIDetail.Id
        public int ScheduleIIISectionId { get; set; }
        public int ScheduleIIISectionItemId { get; set; }
        public int DisplayOrder { get; set; }
        public int IsActive { get; set; }                    // 1 = Active, 0 = Inactive
    }
}
