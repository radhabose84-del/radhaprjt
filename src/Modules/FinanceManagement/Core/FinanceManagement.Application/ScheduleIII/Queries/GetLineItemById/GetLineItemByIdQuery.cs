using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetLineItemById
{
    public class GetLineItemByIdQuery : IRequest<ScheduleIIISectionItemDto?>
    {
        public int Id { get; set; }
    }
}
