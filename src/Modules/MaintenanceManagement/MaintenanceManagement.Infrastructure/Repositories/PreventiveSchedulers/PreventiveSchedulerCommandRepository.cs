#nullable disable
// using Contracts.Events.Maintenance;
// using Contracts.Interfaces.External.IMaintenance;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static MaintenanceManagement.Domain.Common.BaseEntity;
using static MaintenanceManagement.Domain.Common.MiscEnumEntity;

namespace MaintenanceManagement.Infrastructure.Repositories.PreventiveSchedulers
{
    public class PreventiveSchedulerCommandRepository : IPreventiveSchedulerCommand
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        // private readonly IBackgroundServiceClient _backgroundServiceClient;
        private readonly IPreventiveScheduleLogService _preventiveScheduleLogService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        // private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<PreventiveSchedulerCommandRepository> _logger;


        // private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;
        public PreventiveSchedulerCommandRepository(ApplicationDbContext applicationDbContext, IPreventiveSchedulerQuery preventiveSchedulerQuery,
        IMiscMasterQueryRepository miscMasterQueryRepository, IIPAddressService ipAddressService
        // , IBackgroundServiceClient backgroundServiceClient
         ,IPreventiveScheduleLogService preventiveScheduleLogService, IHttpContextAccessor httpContextAccessor,
        //   IEventPublisher eventPublisher,
         ILogger<PreventiveSchedulerCommandRepository> logger)
        {
            _applicationDbContext = applicationDbContext;
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _ipAddressService = ipAddressService;
            // _backgroundServiceClient = backgroundServiceClient;
            _preventiveScheduleLogService = preventiveScheduleLogService;
            _httpContextAccessor = httpContextAccessor;
            // _eventPublisher = eventPublisher;
            _logger = logger;

        }

        public async Task<PreventiveSchedulerHeader> CreateAsync(PreventiveSchedulerHeader preventiveSchedulerHdr)
        {
            preventiveSchedulerHdr.CompanyId = _ipAddressService.GetCompanyId() ?? 0;
            preventiveSchedulerHdr.UnitId = _ipAddressService.GetUnitId() ?? 0;
            _applicationDbContext.Entry(preventiveSchedulerHdr);
            await _applicationDbContext.PreventiveSchedulerHdr.AddAsync(preventiveSchedulerHdr);
            await _applicationDbContext.SaveChangesAsync();

            return preventiveSchedulerHdr;
        }

        public async Task<PreventiveSchedulerDetail> CreateDetailAsync(PreventiveSchedulerDetail preventiveSchedulerDetail)
        {
            await _applicationDbContext.PreventiveSchedulerDtl.AddAsync(preventiveSchedulerDetail);
            await _applicationDbContext.SaveChangesAsync();
            return preventiveSchedulerDetail;
        }

        public async Task<bool> DeleteAsync(int id, PreventiveSchedulerHeader preventiveSchedulerHdr)
        {
            var PreventiveSchedulerToDelete = await _applicationDbContext.PreventiveSchedulerHdr.FirstOrDefaultAsync(u => u.Id == id);
            if (PreventiveSchedulerToDelete != null)
            {
                PreventiveSchedulerToDelete.IsDeleted = IsDelete.Deleted;
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<List<PreventiveSchedulerDetail>> UpdateScheduleDetails(int HeaderId, List<PreventiveSchedulerDetail> preventiveSchedulerDetail)
        {

            var existingPreventiveScheduler = await _applicationDbContext.PreventiveSchedulerDtl
        .Where(ps => ps.PreventiveSchedulerHeaderId == HeaderId)
        .ToListAsync();

            if (existingPreventiveScheduler == null || !existingPreventiveScheduler.Any())
                return null;

            foreach (var updated in preventiveSchedulerDetail)
            {
                var detail = existingPreventiveScheduler.FirstOrDefault(x => x.Id == updated.Id);
                if (detail == null)
                    continue;

                detail.WorkOrderCreationStartDate = updated.WorkOrderCreationStartDate;
                detail.ActualWorkOrderDate = updated.ActualWorkOrderDate;
                detail.MaterialReqStartDays = updated.MaterialReqStartDays;
                detail.ScheduleId = updated.ScheduleId;
                detail.FrequencyTypeId = updated.FrequencyTypeId;
                detail.FrequencyInterval = updated.FrequencyInterval;
                detail.FrequencyUnitId = updated.FrequencyUnitId;
                detail.DownTimeEstimateHrs = updated.DownTimeEstimateHrs;
                detail.GraceDays = updated.GraceDays;
                detail.IsDownTimeRequired = updated.IsDownTimeRequired;
                detail.ReminderMaterialReqDays = updated.ReminderMaterialReqDays;
                detail.ReminderWorkOrderDays = updated.ReminderWorkOrderDays;
                detail.HangfireJobId = updated.HangfireJobId;
            }
            await _applicationDbContext.SaveChangesAsync();

            return existingPreventiveScheduler;
        }

        public async Task<bool> UpdateDetailAsync(int id, string HangfireJobId)
        {
            var existingPreventiveScheduler = await _applicationDbContext.PreventiveSchedulerDtl.FirstOrDefaultAsync(u => u.Id == id);

            if (existingPreventiveScheduler != null)
            {
                existingPreventiveScheduler.HangfireJobId = HangfireJobId;
                _applicationDbContext.PreventiveSchedulerDtl.Update(existingPreventiveScheduler);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;

        }
        public async Task<bool> UpdateRescheduleDate(int id, DateOnly RescheduleDate)
        {
            var existingPreventiveScheduler = await _applicationDbContext.PreventiveSchedulerDtl.FirstOrDefaultAsync(u => u.Id == id);

            if (existingPreventiveScheduler != null)
            {
                existingPreventiveScheduler.ActualWorkOrderDate = RescheduleDate;
                _applicationDbContext.PreventiveSchedulerDtl.Update(existingPreventiveScheduler);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;

        }
        public async Task<PreventiveSchedulerDetail> CreateNextSchedulerDetailAsync(int Id)
        {
            await _preventiveScheduleLogService.CaptureLogs(null, Id, "Create Next Schedule", JsonConvert.SerializeObject(Id));
            var existingPreventiveScheduler = await _applicationDbContext.PreventiveSchedulerDtl
            .AsNoTracking()
            .Where(d => d.Id == Id &&
                    d.IsActive == Status.Active &&
                    d.IsDeleted == IsDelete.NotDeleted &&
                    d.PreventiveScheduler.IsActive == Status.Active &&
                    d.PreventiveScheduler.IsDeleted == IsDelete.NotDeleted)
        .FirstOrDefaultAsync();


            if (existingPreventiveScheduler != null)
            {
                DateTimeOffset? lastMaintenanceDate = await _preventiveSchedulerQuery.GetLastMaintenanceDateAsync(existingPreventiveScheduler.MachineId, Id, WOStatus.MiscCode, MaintenanceStatusUpdate.Code);

                // var headerInfo = await _preventiveSchedulerQuery.GetByIdAsync(existingPreventiveScheduler.PreventiveSchedulerHeaderId);
                var frequencytype = await _miscMasterQueryRepository.GetByIdAsync(existingPreventiveScheduler.FrequencyTypeId ?? 0);
                if (frequencytype.Code != FrequencyType.Code)
                {

                    var miscdetail = await _miscMasterQueryRepository.GetByIdAsync(existingPreventiveScheduler.FrequencyUnitId ?? 0);
                    if (lastMaintenanceDate == null)
                    {
                        _logger.LogError("Next preventive schedule created. PreventiveSchedule: {PreventiveSchedulerDetailId}",
                        Id);
                        throw new Exception("Last maintenance date is null, cannot proceed.");
                    }
                        


                    var (nextDate, reminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate(lastMaintenanceDate.Value.DateTime, existingPreventiveScheduler.FrequencyInterval, miscdetail.Code ?? "", existingPreventiveScheduler.ReminderWorkOrderDays);
                    var (ItemNextDate, ItemReminderDate) = await _preventiveSchedulerQuery.CalculateNextScheduleDate(lastMaintenanceDate.Value.DateTime, existingPreventiveScheduler.FrequencyInterval, miscdetail.Code ?? "", existingPreventiveScheduler.ReminderMaterialReqDays);


                    existingPreventiveScheduler.Id = 0;
                    existingPreventiveScheduler.WorkOrderCreationStartDate = DateOnly.FromDateTime(reminderDate);
                    existingPreventiveScheduler.ActualWorkOrderDate = DateOnly.FromDateTime(nextDate);
                    existingPreventiveScheduler.MaterialReqStartDays = DateOnly.FromDateTime(ItemReminderDate);
                    existingPreventiveScheduler.LastMaintenanceActivityDate = DateOnly.FromDateTime(lastMaintenanceDate.Value.DateTime);
                    existingPreventiveScheduler.IsActive = Status.Active;
                    existingPreventiveScheduler.IsDeleted = IsDelete.NotDeleted;
                    existingPreventiveScheduler.HangfireJobId = null;

                    // var newDtl = new PreventiveSchedulerDetail
                    // {
                    //     PreventiveSchedulerHeaderId = existingPreventiveScheduler.PreventiveSchedulerHeaderId,
                    //     MachineId = existingPreventiveScheduler.MachineId,
                    //     WorkOrderCreationStartDate = DateOnly.FromDateTime(reminderDate),
                    //     ActualWorkOrderDate = DateOnly.FromDateTime(nextDate),
                    //     MaterialReqStartDays = DateOnly.FromDateTime(ItemReminderDate),
                    //     RescheduleReason = null,
                    //     HangfireJobId = null,
                    //     LastMaintenanceActivityDate = DateOnly.FromDateTime(lastMaintenanceDate.Value.DateTime),
                    //     ScheduleId = existingPreventiveScheduler.ScheduleId,
                    //     FrequencyTypeId = existingPreventiveScheduler.FrequencyTypeId,
                    //     FrequencyInterval = existingPreventiveScheduler.FrequencyInterval,
                    //     FrequencyUnitId = existingPreventiveScheduler.FrequencyUnitId,
                    //     GraceDays = existingPreventiveScheduler.GraceDays,
                    //     ReminderWorkOrderDays = existingPreventiveScheduler.ReminderWorkOrderDays,
                    //     ReminderMaterialReqDays = existingPreventiveScheduler.ReminderMaterialReqDays,
                    //     DownTimeEstimateHrs = existingPreventiveScheduler.DownTimeEstimateHrs,
                    //     IsDownTimeRequired = existingPreventiveScheduler.IsDownTimeRequired,
                    //     IsActive = Status.Active,
                    //     IsDeleted = IsDelete.NotDeleted,
                    //     CreatedBy = existingPreventiveScheduler.CreatedBy
                    // }



                    await _applicationDbContext.PreventiveSchedulerDtl.AddAsync(existingPreventiveScheduler);
                    var newDtl = await _applicationDbContext.SaveChangesAsync();
                    // var delay = existingPreventiveScheduler.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue) - DateTime.Today;
                    // var targetDateTime = existingPreventiveScheduler.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
                    // var delay = targetDateTime - DateTime.Now;
                    // string newJobId;
                    // var delayInMinutes = (int)delay.TotalMinutes;

                    // if (delay.TotalSeconds > 0)
                    // {
                    //     // newJobId = await _backgroundServiceClient.ScheduleWorkOrder(existingPreventiveScheduler.Id, delayInMinutes, token);
                    //     var correlationId = Guid.NewGuid();
                    //     var @event = new NextSchedulerCreatedEvent
                    //     {
                    //         CorrelationId = correlationId,
                    //         PreventiveSchedulerDetailId = existingPreventiveScheduler.Id,
                    //         DelayInMinutes = delayInMinutes

                    //     };

                    //     await _eventPublisher.SaveEventAsync(@event);
                    //     await _eventPublisher.PublishPendingEventsAsync();

                    // }
                    // else
                    // {

                    //     // newJobId = await _backgroundServiceClient.ScheduleWorkOrder(existingPreventiveScheduler.Id, 5, token);
                    //       var correlationId = Guid.NewGuid();
                    //        var @event = new NextSchedulerCreatedEvent
                    //        {
                    //            CorrelationId = correlationId,
                    //            PreventiveSchedulerDetailId = existingPreventiveScheduler.Id,
                    //            DelayInMinutes = 5

                    //        };

                    //        await _eventPublisher.SaveEventAsync(@event);
                    //        await _eventPublisher.PublishPendingEventsAsync();
                    // }

                    //     existingPreventiveScheduler.HangfireJobId = newJobId;

                    // _applicationDbContext.PreventiveSchedulerDtl.Update(existingPreventiveScheduler);
                    // await _applicationDbContext.SaveChangesAsync();
                    return existingPreventiveScheduler;
                }

                
            }


            return null;
        }

        public async Task<bool> ScheduleInActive(PreventiveSchedulerHeader preventiveSchedulerHdr)
        {
            var existingPreventiveScheduler = await _applicationDbContext.PreventiveSchedulerHdr
           .FirstOrDefaultAsync(ps => ps.Id == preventiveSchedulerHdr.Id);

            if (existingPreventiveScheduler != null)
            {
                existingPreventiveScheduler.IsActive = preventiveSchedulerHdr.IsActive;
                _applicationDbContext.PreventiveSchedulerHdr.Update(existingPreventiveScheduler);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<bool> DeleteDetailAsync(int id)
        {
            var PreventiveSchedulerToDelete = await _applicationDbContext.PreventiveSchedulerDtl.FirstOrDefaultAsync(u => u.PreventiveSchedulerHeaderId == id);
            if (PreventiveSchedulerToDelete != null)
            {
                PreventiveSchedulerToDelete.IsDeleted = IsDelete.Deleted;
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<bool> RescheduleWithoutWorkOrderAsync(int Id, DateOnly RescheduleDate, CancellationToken cancellationToken)
        {
            var existingPreventiveScheduler = await _applicationDbContext.PreventiveSchedulerDtl
            .Include(ps => ps.PreventiveScheduler)
            .FirstOrDefaultAsync(u =>
                u.Id == Id &&
                u.IsActive == Status.Active &&
                u.IsDeleted == IsDelete.NotDeleted);

            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
            if (existingPreventiveScheduler != null)
            {

                existingPreventiveScheduler.ActualWorkOrderDate = RescheduleDate;
                existingPreventiveScheduler.WorkOrderCreationStartDate = RescheduleDate;

                // _backgroundServiceClient.RemoveHangFireJob(existingPreventiveScheduler.HangfireJobId,token);

                // var delay = existingPreventiveScheduler.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue) - DateTime.Today;
                var targetDateTime = existingPreventiveScheduler.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
                var delay = targetDateTime - DateTime.Now;
                var delayInMinutes = (int)delay.TotalMinutes;

                // if (delay.TotalSeconds > 0)
                // {
                //     newJobId = await _backgroundServiceClient.ScheduleWorkOrder(existingPreventiveScheduler.Id, delayInMinutes, token);
                // }
                // else
                // {

                //     newJobId = await _backgroundServiceClient.ScheduleWorkOrder(existingPreventiveScheduler.Id, 5, token);
                // }
                // existingPreventiveScheduler.HangfireJobId = newJobId;
                _applicationDbContext.PreventiveSchedulerDtl.Update(existingPreventiveScheduler);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }


            return false;
        }

        public async Task<PreventiveSchedulerDetail> GetDetailByMachineActivityAndUnitAsync(string machineCode, string activityName, int unitId)
        {
            return await (
           from m in _applicationDbContext.MachineMaster
           join psd in _applicationDbContext.PreventiveSchedulerDtl on m.Id equals psd.MachineId
           join psa in _applicationDbContext.PreventiveSchedulerActivity on psd.PreventiveSchedulerHeaderId equals psa.PreventiveSchedulerHeaderId
           join am in _applicationDbContext.ActivityMaster on psa.ActivityId equals am.Id
           join psh in _applicationDbContext.PreventiveSchedulerHdr on psd.PreventiveSchedulerHeaderId equals psh.Id
           where m.MachineCode == machineCode
                 && am.ActivityName == activityName
                 && psh.UnitId == unitId
           select psd
       ).FirstOrDefaultAsync();
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<PreventiveSchedulerHeader>> BulkImportPreventiveHeaderAsync(List<PreventiveSchedulerHeader> preventiveSchedulerHeaders)
        {
            await _applicationDbContext.PreventiveSchedulerHdr.AddRangeAsync(preventiveSchedulerHeaders);
            await _applicationDbContext.SaveChangesAsync();
            return preventiveSchedulerHeaders;
        }

        public async Task<PreventiveSchedulerHeader> UpdateScheduleMetadata(PreventiveSchedulerHeader preventiveSchedulerHdr)
        {
            var existingPreventiveScheduler = await _applicationDbContext.PreventiveSchedulerHdr
            .Include(ps => ps.PreventiveSchedulerActivities)
            .Include(ps => ps.PreventiveSchedulerItems)
            .FirstOrDefaultAsync(ps => ps.Id == preventiveSchedulerHdr.Id);


            if (existingPreventiveScheduler != null)
            {


                _applicationDbContext.PreventiveSchedulerActivity.RemoveRange(
                    _applicationDbContext.PreventiveSchedulerActivity.Where(x => x.PreventiveSchedulerHeaderId == preventiveSchedulerHdr.Id));

                _applicationDbContext.PreventiveSchedulerItems.RemoveRange(
                    _applicationDbContext.PreventiveSchedulerItems.Where(x => x.PreventiveSchedulerHeaderId == preventiveSchedulerHdr.Id));

                existingPreventiveScheduler.MachineGroupId = preventiveSchedulerHdr.MachineGroupId;
                existingPreventiveScheduler.DepartmentId = preventiveSchedulerHdr.DepartmentId;
                existingPreventiveScheduler.MaintenanceCategoryId = preventiveSchedulerHdr.MaintenanceCategoryId;
                existingPreventiveScheduler.GraceDays = preventiveSchedulerHdr.GraceDays;
                existingPreventiveScheduler.ReminderWorkOrderDays = preventiveSchedulerHdr.ReminderWorkOrderDays;
                existingPreventiveScheduler.ReminderMaterialReqDays = preventiveSchedulerHdr.ReminderMaterialReqDays;
                existingPreventiveScheduler.IsDownTimeRequired = preventiveSchedulerHdr.IsDownTimeRequired;
                existingPreventiveScheduler.DownTimeEstimateHrs = preventiveSchedulerHdr.DownTimeEstimateHrs;
                existingPreventiveScheduler.ScheduleId = preventiveSchedulerHdr.ScheduleId;
                existingPreventiveScheduler.FrequencyTypeId = preventiveSchedulerHdr.FrequencyTypeId;
                existingPreventiveScheduler.FrequencyInterval = preventiveSchedulerHdr.FrequencyInterval;
                existingPreventiveScheduler.FrequencyUnitId = preventiveSchedulerHdr.FrequencyUnitId;
                existingPreventiveScheduler.PreventiveSchedulerName = preventiveSchedulerHdr.PreventiveSchedulerName;

                _applicationDbContext.PreventiveSchedulerHdr.Update(existingPreventiveScheduler);

                if (preventiveSchedulerHdr.PreventiveSchedulerActivities?.Any() == true)
                    await _applicationDbContext.PreventiveSchedulerActivity.AddRangeAsync(preventiveSchedulerHdr.PreventiveSchedulerActivities);

                if (preventiveSchedulerHdr.PreventiveSchedulerItems?.Any() == true)
                    await _applicationDbContext.PreventiveSchedulerItems.AddRangeAsync(preventiveSchedulerHdr.PreventiveSchedulerItems);


                await _applicationDbContext.SaveChangesAsync();
                return preventiveSchedulerHdr;
            }

            return preventiveSchedulerHdr;
        }

        public async Task<PreventiveSchedulerDetail> UpdateScheduleDetails(PreventiveSchedulerDetail preventiveSchedulerDetail)
        {
            var existingPreventiveScheduler = await _applicationDbContext.PreventiveSchedulerDtl
          .FirstOrDefaultAsync(ps => ps.Id == preventiveSchedulerDetail.Id);

            if (existingPreventiveScheduler == null)
                return null;

            existingPreventiveScheduler.WorkOrderCreationStartDate = preventiveSchedulerDetail.WorkOrderCreationStartDate;
            existingPreventiveScheduler.ActualWorkOrderDate = preventiveSchedulerDetail.ActualWorkOrderDate;
            existingPreventiveScheduler.MaterialReqStartDays = preventiveSchedulerDetail.MaterialReqStartDays;
            existingPreventiveScheduler.FrequencyInterval = preventiveSchedulerDetail.FrequencyInterval;
            existingPreventiveScheduler.IsActive = preventiveSchedulerDetail.IsActive;
            existingPreventiveScheduler.HangfireJobId = preventiveSchedulerDetail.HangfireJobId;
            existingPreventiveScheduler.LastMaintenanceActivityDate = preventiveSchedulerDetail.LastMaintenanceActivityDate;

            await _applicationDbContext.SaveChangesAsync();

            return existingPreventiveScheduler;
        }
         public async Task<bool> DeleteDetailByDetailId(int id)
        {
            var PreventiveSchedulerToDelete = await _applicationDbContext.PreventiveSchedulerDtl.FirstOrDefaultAsync(u => u.Id == id);
            if (PreventiveSchedulerToDelete != null)
            {
                PreventiveSchedulerToDelete.IsDeleted = IsDelete.Deleted;
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}