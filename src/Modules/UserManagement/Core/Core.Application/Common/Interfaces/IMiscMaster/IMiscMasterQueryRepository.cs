using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterQueryRepository
    {
        Task<(List<Core.Domain.Entities.MiscMaster>, int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<Core.Domain.Entities.MiscMaster> GetByIdAsync(int id);
        Task<List<Core.Domain.Entities.MiscMaster>> GetMiscMaster(string searchPattern, string miscTypeCode);
        Task<Core.Domain.Entities.MiscMaster?> GetByMiscMasterCodeAsync(string name, int? id = null);

        // Task<bool> AlreadyExistsAsync(string code,int? id = null);
        Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> FKColumnValidation(int ShiftMasterId);
        Task<Core.Domain.Entities.MiscMaster> GetMiscMasterByName(string miscTypeCode, string miscTypeName);   
    }
}