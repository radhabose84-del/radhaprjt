namespace Contracts.Interfaces.External.IMaintenance
{
    public interface IPreventiveSchedule
    {
        Task<bool> ScheduleWorkOrder();
    }
}