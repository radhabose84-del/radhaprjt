using System.Data;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Domain.Common;
using Dapper;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;
using MaintenanceManagement.Application.Common;
using MediatR;
// using Contracts.Interfaces.External.IUser;
using Microsoft.AspNetCore.SignalR;
using MaintenanceManagement.Application.Common.RealTimeNotificationHub;
using Serilog;

namespace MaintenanceManagement.Infrastructure.Repositories.WorkOrder
{
    public class WorkOrderCommandRepository : IWorkOrderCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IIPAddressService _ipAddressService;
        private readonly IDbConnection _dbConnection;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<WorkOrderCommandRepository> _logger;
        // private readonly ICompanyGrpcClient _companyGrpcClient;     
        // private readonly IUnitGrpcClient _unitGrpcClient;    
        private readonly ITimeZoneService _timeZoneService;

        public WorkOrderCommandRepository(ApplicationDbContext applicationDbContext, IIPAddressService ipAddressService, IDbConnection dbConnection,
        IPublishEndpoint publishEndpoint, ILogger<WorkOrderCommandRepository> logger
        // , ICompanyGrpcClient companyGrpcClient, IUnitGrpcClient unitGrpcClient
        , ITimeZoneService timeZoneService)
        {
            _applicationDbContext = applicationDbContext;
            _ipAddressService = ipAddressService;
            _dbConnection = dbConnection;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            // _companyGrpcClient = companyGrpcClient;
            // _unitGrpcClient = unitGrpcClient;
            _timeZoneService = timeZoneService;
        }
        public async Task<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder> CreateAsync(MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder workOrder, int requestTypeId, CancellationToken cancellationToken)
        {
            var entry = _applicationDbContext.Entry(workOrder);
            workOrder.WorkOrderDocNo = await GetLatestWorkOrderDocNo(requestTypeId);
            await _applicationDbContext.WorkOrder.AddAsync(workOrder);
            await _applicationDbContext.SaveChangesAsync();
            return workOrder;
        }
        public async Task<string?> GetLatestWorkOrderDocNo(int TypeId)
        {
            var companyId = _ipAddressService.GetCompanyId();
            var unitId = _ipAddressService.GetUnitId();
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@UnitId", unitId);
            parameters.Add("@TypeId", TypeId);
            var newAssetCode = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                "dbo.Usp_GetWorkOrderDocNo",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 120);
            return newAssetCode;
        }
        public async Task<bool> UpdateAsync(int workOrderId, MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder workOrder)
        {
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            TimeZoneInfo systemTimeZone;
            try
            {
                systemTimeZone = TimeZoneInfo.FindSystemTimeZoneById(systemTimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                // Common Windows ↔ IANA mismatch handling
                if (string.Equals(systemTimeZoneId, "India Standard Time", StringComparison.OrdinalIgnoreCase))
                    systemTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
                else
                    systemTimeZone = TimeZoneInfo.Local;
            }

            var oldUnitId = _ipAddressService.GetOldUnitId();
            var existingWorkOrder = await _applicationDbContext.WorkOrder
                  .Include(cf => cf.WorkOrderItems)
                  .Include(cf => cf.WorkOrderActivities)
                  .Include(cf => cf.WorkOrderTechnicians)
                  .Include(cf => cf.WorkOrderCheckLists)
                  .FirstOrDefaultAsync(u => u.Id == workOrderId);

            if (existingWorkOrder == null)
                return false;

            _applicationDbContext.WorkOrderActivity.RemoveRange(
                _applicationDbContext.WorkOrderActivity.Where(x => x.WorkOrderId == workOrderId));

            _applicationDbContext.WorkOrderCheckList.RemoveRange(
                _applicationDbContext.WorkOrderCheckList.Where(x => x.WorkOrderId == workOrderId));

            _applicationDbContext.WorkOrderItem.RemoveRange(
                _applicationDbContext.WorkOrderItem.Where(x => x.WorkOrderId == workOrderId));

            _applicationDbContext.WorkOrderTechnician.RemoveRange(
                _applicationDbContext.WorkOrderTechnician.Where(x => x.WorkOrderId == workOrderId));

            _applicationDbContext.StockLedger.RemoveRange(
                _applicationDbContext.StockLedger.Where(x => x.DocNo == workOrderId && new[] { "ISS", "REU", "SRP" }.Contains(x.TransactionType)));

            existingWorkOrder.DowntimeStart = workOrder.DowntimeStart;
            existingWorkOrder.DowntimeEnd = workOrder.DowntimeEnd;
            existingWorkOrder.Image = workOrder.Image;
            existingWorkOrder.Remarks = workOrder.Remarks;
            existingWorkOrder.StatusId = workOrder.StatusId;
            existingWorkOrder.RootCauseId = workOrder.RootCauseId;
            existingWorkOrder.ModifiedBy = _ipAddressService.GetUserId();
            existingWorkOrder.ModifiedByName = _ipAddressService.GetUserName();
            existingWorkOrder.ModifiedIP = _ipAddressService.GetSystemIPAddress();
            existingWorkOrder.ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, systemTimeZone);

            _applicationDbContext.WorkOrder.Update(existingWorkOrder);

            await _applicationDbContext.AddRangeAsync(workOrder.WorkOrderActivities ?? []);
            await _applicationDbContext.AddRangeAsync(workOrder.WorkOrderItems ?? []);

            await _applicationDbContext.AddRangeAsync(workOrder.WorkOrderTechnicians ?? []);
            await _applicationDbContext.AddRangeAsync(workOrder.WorkOrderCheckLists ?? []);
            var result = await _applicationDbContext.SaveChangesAsync();
            var cancelledStatusId = await _applicationDbContext.MiscMaster
                .Where(x => x.Code == MiscEnumEntity.MaintenanceStatusCancelled.Code)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
            if (workOrder.StatusId != cancelledStatusId)
            {
                int docSerialNumber = 1;
                foreach (var item in workOrder.WorkOrderItems ?? [])
                {
                    if ((item.UsedQty > 0) || (item.ToSubStoreQty > 0) || (item.ScarpQty > 0))
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@OldUnitCode", oldUnitId);
                        parameters.Add("@DocNo", workOrder.Id);
                        parameters.Add("@DocSNo", docSerialNumber);
                        parameters.Add("@ItemCode", item.OldItemCode);
                        parameters.Add("@ItemName", item.ItemName);
                        parameters.Add("@UsedQty", item.UsedQty);
                        parameters.Add("@SubStoreQty", item.ToSubStoreQty);
                        parameters.Add("@ScrapQty", item.ScarpQty);
                        parameters.Add("@Rate", item.Rate);
                        parameters.Add("@DepartmentId", item.DepartmentId);

                        await _dbConnection.ExecuteAsync(
                            "usp_InsertStockLedger",  // your stored procedure name
                            parameters,
                            commandType: CommandType.StoredProcedure
                        );
                    }
                    string tempItemFilePath = item.Image;
                    if (tempItemFilePath != null)
                    {
                        string baseDirectory = await GetBaseDirectoryItemAsync();

                        // var companies = await _companyGrpcClient.GetAllCompanyAsync();
                        // var units = await _unitGrpcClient.GetAllUnitAsync();

                        // var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
                        // var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                        // var companyName = companyLookup.TryGetValue(workOrder.CompanyId, out var cname) ? cname : string.Empty;
                        // var unitName = unitLookup.TryGetValue(workOrder.UnitId, out var uname) ? uname : string.Empty;

                        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory
                        // , companyName, unitName
                        );
                        string filePath = Path.Combine(uploadPath, tempItemFilePath);
                        EnsureDirectoryExists(Path.GetDirectoryName(filePath));

                        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
                            string newFileName = $"{workOrder.WorkOrderDocNo}-{docSerialNumber}{Path.GetExtension(tempItemFilePath)}";
                            string newFilePath = Path.Combine(directory, newFileName);

                            try
                            {
                                File.Move(filePath, newFilePath);
                                //assetEntity.AssetImage = newFileName;
                                await UpdateWOItemImageAsync(item.Id, newFileName);
                            }
                            catch (Exception ex)
                            {
                                Log.Information(ex, "Failed to rename file.");
                            }
                        }
                    }
                    docSerialNumber++;
                }
            }
            return result > 0;
        }
        private void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public async Task<string> GetBaseDirectoryItemAsync()
        {
            const string query = @"
            SELECT Description AS BaseDirectory  
                FROM Maintenance.MiscTypeMaster 
                WHERE MiscTypeCode='WOItemImage'  
                AND IsDeleted=0
            ";
            var result = await _dbConnection.QueryFirstOrDefaultAsync<string>(query);
            return result;
        }      
        public async Task<bool> RemoveWOImageReferenceAsync(int workOrderId)
        {
            var asset = await _applicationDbContext.WorkOrder.FindAsync(workOrderId);
            if (asset == null)
            {
                return false;  // Asset not found
            }

            asset.Image = null;
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> CreateScheduleAsync(int workOrderId, MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule workOrderSchedule)
        {
            await _applicationDbContext.WorkOrderSchedule.AddAsync(workOrderSchedule);
            await _applicationDbContext.SaveChangesAsync();

            // Check if this is the only schedule for the given WorkOrderId
            var existingScheduleCount = await _applicationDbContext.WorkOrderSchedule
                .CountAsync(ws => ws.WorkOrderId == workOrderId);
            // If it's the first schedule, update MaintenanceRequest status
            if (existingScheduleCount == 1)
            {
                var workOrder = await _applicationDbContext.WorkOrder
                .FirstOrDefaultAsync(wo => wo.Id == workOrderId);

                var status = await _applicationDbContext.MiscMaster
                .FirstOrDefaultAsync(mm => mm.Code == MiscEnumEntity.GetStatusId.Status);


                workOrder.StatusId = status.Id; // Start work
                _applicationDbContext.WorkOrder.Update(workOrder);
                await _applicationDbContext.SaveChangesAsync();

                var maintenanceRequest = await _applicationDbContext.MaintenanceRequest
                    .FirstOrDefaultAsync(mr => mr.Id == workOrder.RequestId);

                if (maintenanceRequest != null)
                {
                    maintenanceRequest.RequestStatusId = status.Id; // Start work
                    _applicationDbContext.MaintenanceRequest.Update(maintenanceRequest);
                    await _applicationDbContext.SaveChangesAsync();
                }
            }
            return workOrderSchedule.Id;
        }
        public async Task<bool> UpdateScheduleAsync(int workOrderId, MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrderSchedule workOrderSchedule)
        {
            //var existingWO =await _applicationDbContext.WorkOrderSchedule.FirstOrDefaultAsync(m =>m.WorkOrderId == workOrderId);
            var existingWO = await _applicationDbContext.WorkOrderSchedule
                .Where(m => m.WorkOrderId == workOrderId)
                .OrderByDescending(m => m.Id)
                .FirstOrDefaultAsync();
            if (existingWO != null)
            {
                existingWO.EndTime = workOrderSchedule.EndTime;                
                existingWO.IsCompleted = workOrderSchedule.IsCompleted;
                existingWO.StatusId = workOrderSchedule.StatusId;
                _applicationDbContext.WorkOrderSchedule.Update(existingWO);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
        public async Task<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder> GetByIdAsync(int workOrderId)
        {
            return await _applicationDbContext.WorkOrder
                      .FirstOrDefaultAsync(x => x.Id == workOrderId);
        }
        public async Task<bool> UpdateWOImageAsync(int workOrderId, string imageName)
        {
            var workOrder = await _applicationDbContext.WorkOrder.FindAsync(workOrderId);
            if (workOrder == null)
            {
                return false;
            }
            workOrder.Image = imageName;
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteWOImageAsync(string imageName)
        {
            var workOrder = await _applicationDbContext.WorkOrder.FirstOrDefaultAsync(x => x.Image == imageName);
            if (workOrder == null)
            {
                return false;
            }
            workOrder.Image = "";
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateWOItemImageAsync(int workOrderId, string imageName)
        {
            var workOrder = await _applicationDbContext.WorkOrderItem.FindAsync(workOrderId);
            if (workOrder == null)
            {
                return false;
            }
            workOrder.Image = imageName;
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteItemImageAsync(string imageName)
        {
            var workOrder = await _applicationDbContext.WorkOrderItem.FirstOrDefaultAsync(x => x.Image == imageName);
            if (workOrder == null)
            {
                return false;
            }
            workOrder.Image = "";
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<MaintenanceManagement.Domain.Entities.MiscMaster?> GetMiscMasterByCodeAsync(string code)
        {
            return await _applicationDbContext.MiscMaster
                .FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<bool> RevertWorkOrderStatusAsync(int workOrderId)
        {
            var workOrder = await _applicationDbContext.WorkOrder.FindAsync(workOrderId);
            // var preventiveSchedulerDetail = await _applicationDbContext.PreventiveSchedulerDtl.FindAsync(workOrder?.PreventiveScheduleId);

            if (workOrder == null )
            {
                _logger.LogWarning("⚠️ Work order not found for rollback. ID: {id}", workOrderId);
                return false;
            }
            var openStatusId = await _applicationDbContext.MiscMaster
            .Where(x => x.Code == MiscEnumEntity.StatusOpen.Code)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

            workOrder.StatusId = openStatusId;
            workOrder.ModifiedDate = DateTime.UtcNow;

            _applicationDbContext.WorkOrder.Update(workOrder);

            // preventiveSchedulerDetail.IsDeleted = BaseEntity.IsDelete.Deleted;

            // _applicationDbContext.PreventiveSchedulerDtl.Update(preventiveSchedulerDetail);

            await _applicationDbContext.SaveChangesAsync();

            _logger.LogInformation("✅ Work order status reverted for ID: {id}", workOrderId);
            return true;
        }

        public async Task<MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder> CreatePreventiveAsync(MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder workOrder, int requestTypeId, int companyId, int unitId, CancellationToken cancellationToken)
        {

          var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

                try
                {
                    var resultList = await _applicationDbContext
                        .Database
                        .SqlQuery<string>(
                            $"EXEC dbo.Usp_GetWorkOrderDocNo @CompanyId = {companyId}, @UnitId = {unitId}, @TypeId = {requestTypeId}")
                        .ToListAsync(cancellationToken);

                        var result = resultList.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(result))
                        throw new InvalidOperationException("Failed to generate new Work Order Doc No.");

                    workOrder.WorkOrderDocNo = result;

                    await _applicationDbContext.WorkOrder.AddAsync(workOrder, cancellationToken);
                    await _applicationDbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return workOrder;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });

        }

      public async Task<bool> UpdateRequestDateAsync(
            int workOrderId,
            DateTimeOffset requestDate,int isSystemTime,
            CancellationToken cancellationToken = default)
        {
             var tzId = _timeZoneService.GetSystemTimeZone();
            TimeZoneInfo systemTz;
            try
            {
                systemTz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            }
            catch
            {
                systemTz = TimeZoneInfo.Local;
            }

            if (isSystemTime == 1)
            {

                requestDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, systemTz);

            }
            else
            {
                requestDate=TimeZoneInfo.ConvertTime(requestDate, systemTz);               
            }   

            // 1. Get the WorkOrder
            var workOrder = await _applicationDbContext.WorkOrder
                .FirstOrDefaultAsync(x => x.Id == workOrderId, cancellationToken);

            if (workOrder is null)
                return false;

            // 2. Update DowntimeEnd from input parameter
            workOrder.DowntimeEnd = requestDate;

            // 3. Fetch MaintenanceRequest.RequestDate and update DowntimeStart
            if (workOrder.RequestId.HasValue && workOrder.RequestId.Value > 0)
            {
                var mrRequestDate = await _applicationDbContext.MaintenanceRequest
                    .Where(m => m.Id == workOrder.RequestId.Value)
                    .Select(m => m.CreatedDate)     
                    .FirstOrDefaultAsync(cancellationToken);

                
               if (mrRequestDate.HasValue)
                {                    
                    workOrder.DowntimeStart = TimeZoneInfo.ConvertTime(mrRequestDate.Value, systemTz);                    
                }
            }
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}