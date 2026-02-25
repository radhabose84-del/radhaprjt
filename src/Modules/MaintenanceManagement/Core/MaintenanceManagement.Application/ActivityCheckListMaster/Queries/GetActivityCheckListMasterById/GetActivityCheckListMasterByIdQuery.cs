using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster;
using MediatR;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMasterById
{
    public class GetActivityCheckListMasterByIdQuery : IRequest<GetAllActivityCheckListMasterDto>
    {
        public int Id { get; set; }
    }
}