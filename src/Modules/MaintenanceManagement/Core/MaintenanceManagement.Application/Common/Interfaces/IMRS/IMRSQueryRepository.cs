using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.MRS.Queries;
using MaintenanceManagement.Application.MRS.Queries.GetCategory;
using MaintenanceManagement.Application.MRS.Queries.GetDepartment;
using MaintenanceManagement.Application.MRS.Queries.GetPendingQty;
using MaintenanceManagement.Application.MRS.Queries.GetSubCostCenter;
using MaintenanceManagement.Application.MRS.Queries.GetSubDepartment;

namespace MaintenanceManagement.Application.Common.Interfaces.IMRS
{
    public interface IMRSQueryRepository
    {
        Task<List<MDepartmentDto>> GetMDepartment(string OldUnitcode);
        Task<List<MSubCostCenterDto>> GetSubCostCenter(string OldUnitcode);
        Task<List<MCategoryDto>> GetCategory(string OldUnitcode);
        Task<List<MSubDepartment>> GetSubDepartment(string OldUnitcode);
        Task<GetPendingQtyDto?> GetPendingIssueAsync(string oldUnitCode, string itemCode);
       
       
    }
}