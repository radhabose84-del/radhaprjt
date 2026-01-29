using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMasterToDownload;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterQueryRepository
    {
        Task<(List<MaintenanceManagement.Domain.Entities.MiscMaster>, int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<MaintenanceManagement.Domain.Entities.MiscMaster> GetByIdAsync(int id);
        Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMiscMaster(string searchPattern, string miscTypeCode);
        Task<MaintenanceManagement.Domain.Entities.MiscMaster?> GetByMiscMasterCodeAsync(string name, int? id = null);
        Task<MaintenanceManagement.Domain.Entities.MiscMaster?> GetByWFMiscMasterCodeAsync(string name, int? id = null);
        // Task<bool> AlreadyExistsAsync(string code,int? id = null);
        Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> FKColumnValidation(int ShiftMasterId);
        Task<MaintenanceManagement.Domain.Entities.MiscMaster> GetMiscMasterByName(string miscTypeCode, string miscTypeName);     
        
                           
       
            
    }
}