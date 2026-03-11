using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repositories.MaintenanceRequest
{
    public class MaintenanceRequestCommandRepository    : IMaintenanceRequestCommandRepository
    {

         private readonly ApplicationDbContext _dbContext;
         private  readonly IMaintenanceRequestQueryRepository _maintenanceRequestQueryRepository;

         public readonly IWorkOrderCommandRepository _workOrderCommandRepository;

         private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;

         
          public MaintenanceRequestCommandRepository(ApplicationDbContext applicationDbContext , IMaintenanceRequestQueryRepository maintenanceRequest, IWorkOrderCommandRepository workOrderCommandRepository, IIPAddressService ipAddressService, ITimeZoneService timeZoneService)
        {
            _dbContext = applicationDbContext;
            _maintenanceRequestQueryRepository = maintenanceRequest;
            _workOrderCommandRepository = workOrderCommandRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
        }      
        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.MaintenanceRequest maintenanceRequest)
        {
             await _dbContext.MaintenanceRequest.AddAsync(maintenanceRequest);
                await _dbContext.SaveChangesAsync();
                return maintenanceRequest.Id;
        }

        public async Task AddWithoutSaveAsync(MaintenanceManagement.Domain.Entities.MaintenanceRequest maintenanceRequest)
        {
            await _dbContext.MaintenanceRequest.AddAsync(maintenanceRequest);
            // No SaveChangesAsync — caller commits atomically via CommitAsync
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.WorkOrderMaster.WorkOrder workOrder, CancellationToken cancellationToken)
        {
            _dbContext.WorkOrder.Add(workOrder);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
                
         public async Task<bool> UpdateAsync( MaintenanceManagement.Domain.Entities.MaintenanceRequest maintenanceRequest)
        {
                var existingRequest = await _dbContext.MaintenanceRequest
                    .FirstOrDefaultAsync(m => m.Id == maintenanceRequest.Id);

                if (existingRequest != null)
                {
                      bool maintenanceTypeChanged = existingRequest.MaintenanceTypeId != maintenanceRequest.MaintenanceTypeId;

                    existingRequest.RequestTypeId = maintenanceRequest.RequestTypeId;
                    existingRequest.MaintenanceTypeId = maintenanceRequest.MaintenanceTypeId;
                    existingRequest.MachineId = maintenanceRequest.MachineId;
                // existingRequest.CompanyId = maintenanceRequest.CompanyId;
                // existingRequest.UnitId = maintenanceRequest.UnitId;                   
                    existingRequest.ProductionDepartmentId = maintenanceRequest.ProductionDepartmentId;
                    existingRequest.MaintenanceDepartmentId = maintenanceRequest.MaintenanceDepartmentId;
                    existingRequest.SourceId = maintenanceRequest.SourceId;
                    existingRequest.VendorId = maintenanceRequest.VendorId;
                    existingRequest.OldVendorId = maintenanceRequest.OldVendorId;
                    existingRequest.ServiceTypeId = maintenanceRequest.ServiceTypeId;
                    existingRequest.ServiceLocationId = maintenanceRequest.ServiceLocationId;
                    existingRequest.ModeOfDispatchId = maintenanceRequest.ModeOfDispatchId;
                    existingRequest.ExpectedDispatchDate = maintenanceRequest.ExpectedDispatchDate;
                    existingRequest.SparesTypeId = maintenanceRequest.SparesTypeId;
                    existingRequest.EstimatedServiceCost = maintenanceRequest.EstimatedServiceCost;
                    existingRequest.EstimatedSpareCost = maintenanceRequest.EstimatedSpareCost;
                    existingRequest.RequestStatusId = maintenanceRequest.RequestStatusId;
                    existingRequest.Remarks = maintenanceRequest.Remarks; 

                  //  _dbContext.MaintenanceRequest.Update(existingRequest);
                   // return await _dbContext.SaveChangesAsync() > 0;
                 if (maintenanceTypeChanged)
                   {
                    // Get latest WorkOrderDocNo
                    var latestDocNo = await _workOrderCommandRepository.GetLatestWorkOrderDocNo(existingRequest.MaintenanceTypeId); // or MaintenanceTypeId

                    // Find the related WorkOrder
                    var workOrder = await _dbContext.WorkOrder
                        .FirstOrDefaultAsync(w => w.RequestId == maintenanceRequest.Id);

                    if (workOrder != null)
                    {
                          string currentIp = _ipAddressService.GetSystemIPAddress();
                        int userId = _ipAddressService.GetUserId(); 
                        string username = _ipAddressService.GetUserName();
                        var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
                        var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);  

                        workOrder.WorkOrderDocNo = latestDocNo;
                        workOrder.ModifiedDate = DateTimeOffset.UtcNow;                         
                        workOrder.ModifiedBy = userId;
                        workOrder.ModifiedDate = currentTime;
                        workOrder.ModifiedByName = username;
                        workOrder.ModifiedIP = currentIp;

                        _dbContext.WorkOrder.Update(workOrder);
                    }
                 }

                    _dbContext.MaintenanceRequest.Update(existingRequest);
                    return await _dbContext.SaveChangesAsync() > 0;

                }
                

                return false;
        }

  
        public async Task<bool> UpdateStatusAsync(int id)
        {
            
            var WOstatusOpen = await _maintenanceRequestQueryRepository.GetWOclosedOrInProgressAsync(id );
           if (WOstatusOpen)
           return false;

            var statusList = await _maintenanceRequestQueryRepository.GetMaintenancestatusAsync();
            var status = statusList.FirstOrDefault();

            if (status == null)
                return false;

            // Step 2: Find the MaintenanceRequest by ID
            var entity = await _dbContext.MaintenanceRequest.FindAsync(id);
            if (entity == null)
                return false;

            // Step 3: Update with new status ID
            entity.RequestStatusId = status.Id;
            entity.ModifiedDate = DateTime.UtcNow;

            _dbContext.MaintenanceRequest.Update(entity);
            await _dbContext.SaveChangesAsync();

            return true;
        }


    }
}