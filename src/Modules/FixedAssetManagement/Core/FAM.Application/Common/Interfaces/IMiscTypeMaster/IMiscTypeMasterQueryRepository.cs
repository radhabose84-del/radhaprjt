namespace FAM.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterQueryRepository
    {


           Task<(List<FAM.Domain.Entities.MiscTypeMaster>,int)> GetAllMiscTypeMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
           Task<FAM.Domain.Entities.MiscTypeMaster> GetByIdAsync(int id);

            Task<List<FAM.Domain.Entities.MiscTypeMaster>> GetMiscTypeMaster(string searchPattern);

            Task<FAM.Domain.Entities.MiscTypeMaster?> GetByMiscTypeMasterCodeAsync(string name,int? id = null);

           

    }
}