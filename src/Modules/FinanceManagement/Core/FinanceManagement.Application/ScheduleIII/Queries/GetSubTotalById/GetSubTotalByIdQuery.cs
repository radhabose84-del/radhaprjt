using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetSubTotalById
{
    public class GetSubTotalByIdQuery : IRequest<ScheduleIIISubTotalDto?>
    {
        public int Id { get; set; }
    }
}
