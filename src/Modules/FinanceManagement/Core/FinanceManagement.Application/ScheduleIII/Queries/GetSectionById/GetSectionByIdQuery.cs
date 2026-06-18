using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetSectionById
{
    public class GetSectionByIdQuery : IRequest<ScheduleIIISectionDto?>
    {
        public int Id { get; set; }
    }
}
