using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateMaster
{
    // Adds one included line (ScheduleIIIDetail) to the token company/division structure.
    // The header is auto-created (DRAFT) on the first line. CompanyId + DivisionId come from the token.
    public class CreateMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int ScheduleIIISectionId { get; set; }       // the section the line belongs to
        public int ScheduleIIISectionItemId { get; set; }   // the included line
        public int DisplayOrder { get; set; }
    }
}
