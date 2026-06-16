using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetLineItemById
{
    public class GetLineItemByIdQuery : IRequest<ScheduleIIILineItemDto?>
    {
        public int Id { get; set; }
    }
}
