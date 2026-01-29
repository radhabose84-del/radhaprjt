using Microsoft.AspNetCore.SignalR;

namespace MaintenanceManagement.Application.Common.RealTimeNotificationHub
{
    public class WorkOrderScheduleHub : Hub
    {        
       public async Task JoinDepartmentGroup(string departmentName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, departmentName);
        }

        public async Task LeaveDepartmentGroup(string departmentName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, departmentName);
        }
    }
}