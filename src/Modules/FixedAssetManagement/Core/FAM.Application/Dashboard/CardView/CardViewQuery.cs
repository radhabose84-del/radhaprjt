using MediatR;

namespace FAM.Application.Dashboard.CardView
{
    public class CardViewQuery : IRequest<CardViewDto>
    {
        public string? Type { get; set; }

        public int? DepartmentId { get; set; }
    }
}