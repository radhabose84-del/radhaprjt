using MediatR;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetCheckListByActivityId
{
    public class GetActivityCheckListByActivityIdQuery : IRequest<List<GetActivityCheckListByActivityIdDto>>
    {
        public List<int>? Ids { get; set; }
        public int? WorkOrderId { get; set; }  
    }
}