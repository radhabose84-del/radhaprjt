using MaintenanceManagement.Application.ActivityCheckListMaster.Command.CreateActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.DeleteActivityCheckListMaster;
using MaintenanceManagement.Application.ActivityCheckListMaster.Command.UpdateActivityCheckListMaster;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.TestData
{
    public static class ActivityCheckListMasterBuilders
    {
        public static CreateActivityCheckListMasterCommand ValidCreateCommand(
            int activityId = 1,
            string checkList = "Check Oil Level") =>
            new CreateActivityCheckListMasterCommand
            {
                ActivityID = activityId,
                ActivityCheckList = checkList,
                UnitId = 1
            };

        public static UpdateActivityCheckListMasterCommand ValidUpdateCommand(int id = 1) =>
            new UpdateActivityCheckListMasterCommand
            {
                Id = id,
                ActivityChecklist = "Updated Check",
                UnitId = 1
            };

        public static DeleteActivityCheckListMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteActivityCheckListMasterCommand { Id = id };

        public static MaintenanceManagement.Domain.Entities.ActivityCheckListMaster ValidEntity(int id = 1) =>
            new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster
            {
                Id = id,
                ActivityId = id,
                ActivityCheckList = "Check Oil Level",
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
