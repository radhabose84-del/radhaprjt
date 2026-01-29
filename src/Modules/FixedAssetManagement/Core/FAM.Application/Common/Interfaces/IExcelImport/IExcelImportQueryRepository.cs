using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.ExcelImport;
using FAM.Application.ExcelImport;

namespace FAM.Application.Common.Interfaces.IExcelImport
{
    public interface IExcelImportQueryRepository
    {
        Task<int?> GetAssetUnitIdByNameAsync(string unitName);
        Task<int?> GetAssetDeptIdByNameAsync(string deptName);        
        Task<string?> GetCompanyByNameAsync(int companyId);
        Task<UnitDto?> GetUnitByNameAsync(int unitId);     
    }
}