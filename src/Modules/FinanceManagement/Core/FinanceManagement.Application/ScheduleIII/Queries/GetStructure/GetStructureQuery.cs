using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetStructure
{
    public class GetStructureQuery : IRequest<ScheduleIIIStructureDto?>
    {
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
    }
}
