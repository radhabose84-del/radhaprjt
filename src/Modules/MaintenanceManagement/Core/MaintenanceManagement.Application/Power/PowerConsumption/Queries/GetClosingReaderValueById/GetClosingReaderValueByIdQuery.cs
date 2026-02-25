using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetClosingReaderValueById
{
    public class GetClosingReaderValueByIdQuery :  IRequest<GetClosingReaderValueDto>
    {
        public int FeederId { get; set; }
    }
}