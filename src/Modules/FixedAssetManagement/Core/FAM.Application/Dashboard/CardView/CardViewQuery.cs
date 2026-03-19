using MediatR;

namespace FAM.Application.Dashboard.CardView
{
    public class CardViewQuery : IRequest<CardViewDto>
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? Type { get; set; }
        public int? DepartmentId { get; set; }
    }
}