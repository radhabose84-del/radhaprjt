using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetCheckListByActivityId;

namespace MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster
{
    public interface IActivityCheckListMasterQueryRepository
    {
        Task<(List<GetAllActivityCheckListMasterDto>, int)> GetAllActivityCheckListMasterAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<GetAllActivityCheckListMasterDto> GetByIdAsync(int Id);

        Task<bool> GetByActivityCheckListAsync(string activityChecklist, int activityId);

        Task<bool> AlreadyExistsCheckListAsync(string activityChecklist, int activityId, int? id = null);

        // Task<List<GetActivityCheckListByActivityIdDto>> GetCheckListByActivityIdAsync( int  Id) ;
        //    Task<List<GetActivityCheckListByActivityIdDto>> GetCheckListByActivityIdsAsync(List<int> ids);

        Task<List<GetActivityCheckListByActivityIdDto>> GetCheckListByActivityIdsAsync(List<int> ids, int? workOrderId = null);

        //    Task<bool> SoftDeleteValidation(int Id); 

        Task<bool> IsActivityCheckListMasterLinkedAsync(int id);

                                                              

             

    }
}