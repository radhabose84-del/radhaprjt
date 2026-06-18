using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateSection
{
    public class CreateSectionCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? SectionName { get; set; }
        public int StatementTypeId { get; set; }   // MiscMaster (S3_STMT_TYPE)
        public int NatureId { get; set; }           // MiscMaster (S3_NATURE)
    }
}
