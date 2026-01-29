using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace MaintenanceManagement.Infrastructure.Repositories.PreventiveSchedulesLogs
{
    public class PreventiveScheduleLogsService : IPreventiveScheduleLogService
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IIPAddressService _ipAddressService;
         private readonly List<PreventiveScheduleLog> _pendingLogs = new();
        public PreventiveScheduleLogsService(ApplicationDbContext applicationDbContext, IIPAddressService ipAddressService)
        {
            _applicationDbContext = applicationDbContext;
            _ipAddressService = ipAddressService;
        }
        public async Task<bool> CaptureLogs(int? PreventiveScheduleId,int? PreventiveScheduleDetailId,string ActionType,string ChangedFields)
        {
            var preventiveScheduleLog = new PreventiveScheduleLog()
            {
                PreventiveScheduleId = PreventiveScheduleId,
                PreventiveScheduleDetailId = PreventiveScheduleDetailId,
                ActionType = ActionType,
                ChangedFields = ChangedFields,
                CreatedBy = _ipAddressService.GetUserId(),
                CreatedByName = _ipAddressService.GetUserName(),
                CreatedDate = DateTimeOffset.Now,
                CreatedIP = _ipAddressService.GetUserIPAddress()
            };
            

            await _applicationDbContext.PreventiveScheduleLog.AddAsync(preventiveScheduleLog);
            return await _applicationDbContext.SaveChangesAsync() > 0;
            
        }

    }
}