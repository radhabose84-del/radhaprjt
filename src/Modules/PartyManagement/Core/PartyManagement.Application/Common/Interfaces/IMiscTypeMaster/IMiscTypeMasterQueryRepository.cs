using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterQueryRepository
    {
        Task<(List<PartyManagement.Domain.Entities.MiscTypeMaster>,int)> GetAllMiscTypeMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<PartyManagement.Domain.Entities.MiscTypeMaster> GetByIdAsync(int id);

        Task<List<PartyManagement.Domain.Entities.MiscTypeMaster>> GetMiscTypeMaster(string searchPattern);

        Task<PartyManagement.Domain.Entities.MiscTypeMaster?> GetByMiscTypeMasterCodeAsync(string name,int? id = null);

        Task<bool> AlreadyExistsAsync(string miscTypeCode,int? id = null);

        Task<bool> NotFoundAsync(int Id );

        Task<bool> SoftDeleteValidation(int Id); 
    }
}