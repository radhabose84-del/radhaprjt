namespace MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog
{
    public interface IPreventiveScheduleLogService
    {
        Task<bool> CaptureLogs(int? PreventiveScheduleId,int? PreventiveScheduleDetailId,string ActionType,string ChangedFields);
        
    }
}