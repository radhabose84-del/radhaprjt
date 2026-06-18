using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateSection
{
    public class UpdateSectionCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? SectionName { get; set; }
        public int StatementTypeId { get; set; }
        public int NatureId { get; set; }
        public int IsActive { get; set; }           // 1 = Active, 0 = Inactive
    }
}
