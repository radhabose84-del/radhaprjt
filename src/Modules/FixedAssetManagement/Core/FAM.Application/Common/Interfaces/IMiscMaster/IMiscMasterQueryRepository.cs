namespace FAM.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterQueryRepository
  {
    Task<(List<FAM.Domain.Entities.MiscMaster>, int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
    Task<FAM.Domain.Entities.MiscMaster> GetByIdAsync(int id);
    //  Task<List<FAM.Domain.Entities.MiscMaster>> GetMiscMaster(string searchPattern);

    Task<List<FAM.Domain.Entities.MiscMaster>> GetMiscMaster(string miscTypeCode, string miscTypeName);

    // Task<FAM.Domain.Entities.MiscMaster?> GetByMiscMasterCodeAsync(string name,int? id = null);

    Task<FAM.Domain.Entities.MiscMaster?> GetByMiscMasterCodeAsync(string name, int miscTypeId, int? id = null);

    Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null);

    Task<FAM.Domain.Entities.MiscMaster?> GetByMiscTypeIdAndCodeAsync(int miscTypeId, string code);
    
    Task<bool> IsMiscMasterLinkedAsync(int id);
            
    }
}