using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetScheduleIIIById
{
    // Returns the full tree: Structure + Sections (+ nested LineItems) + SubTotals (+ Formulas).
    public class GetScheduleIIIByIdQuery : IRequest<ScheduleIIIStructureDto?>
    {
        public int Id { get; set; }
    }
}
