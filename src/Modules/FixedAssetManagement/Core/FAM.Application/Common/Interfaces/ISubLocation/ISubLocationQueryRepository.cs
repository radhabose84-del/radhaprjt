using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.Common.Interfaces.ISubLocation
{
    public interface ISubLocationQueryRepository
    {
        Task<(List<FAM.Domain.Entities.SubLocation>, int)> GetAllSubLocationAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<FAM.Domain.Entities.SubLocation> GetByIdAsync(int id);
        Task<List<FAM.Domain.Entities.SubLocation>> GetSubLocation(string searchPattern);
        Task<FAM.Domain.Entities.SubLocation?> GetBySubLocationNameAsync(string name, int DepartmentId, int LocationId, int UnitId, int? id = null);
        Task<bool> IsParentLocationActiveAsync(int locationId);
        Task<FAM.Domain.Entities.SubLocation?> GetBySubLocationCodeAsync(string code, int DepartmentId, int LocationId, int UnitId, int? id = null);
        Task<bool> IsSubLocationLinkedAsync(int id);

    }
}