using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.Get03BDropdownPreview
{
    public class Get03BDropdownPreviewQuery : IRequest<Preview03BDto>
    {
        public int ScheduleIIIMasterId { get; set; }
    }
}
