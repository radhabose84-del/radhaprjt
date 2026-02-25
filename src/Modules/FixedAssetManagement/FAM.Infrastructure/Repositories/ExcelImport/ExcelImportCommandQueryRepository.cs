using Contracts.Interfaces.Lookups.Users;
using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Application.ExcelImport;

namespace FAM.Infrastructure.Repositories.ExcelImport
{
    public class ExcelImportCommandQueryRepository  : IExcelImportQueryRepository
    {
        private readonly IUnitLookup _unitLookup;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly ICompanyLookup _companyLookup;

        public ExcelImportCommandQueryRepository(
            IUnitLookup unitLookup, IDepartmentLookup departmentLookup, ICompanyLookup companyLookup)
        {
            _unitLookup = unitLookup;
            _departmentLookup = departmentLookup;
            _companyLookup = companyLookup;
        }

        public async Task<int?> GetAssetDeptIdByNameAsync(string deptName)
        {
            var allDepts = await _departmentLookup.GetAllDepartmentAsync();
            var match = allDepts.FirstOrDefault(d =>
                d.DepartmentName != null &&
                d.DepartmentName.Contains(deptName, StringComparison.OrdinalIgnoreCase));
            return match?.DepartmentId;
        }

        public async Task<int?> GetAssetUnitIdByNameAsync(string unitName)
        {
            var allUnits = await _unitLookup.GetAllUnitAsync();
            var match = allUnits.FirstOrDefault(u =>
                u.UnitName != null &&
                u.UnitName.Contains(unitName, StringComparison.OrdinalIgnoreCase));
            return match?.UnitId;
        }

        public async Task<string?> GetCompanyByNameAsync(int companyId)
        {
            var allCompanies = await _companyLookup.GetAllCompanyAsync();
            var match = allCompanies.FirstOrDefault(c => c.CompanyId == companyId);
            return match?.CompanyName;
        }

        public async Task<UnitDto?> GetUnitByNameAsync(int unitId)
        {
            var allUnits = await _unitLookup.GetAllUnitAsync();
            var match = allUnits.FirstOrDefault(u => u.UnitId == unitId);
            if (match == null) return null;
            return new UnitDto
            {
                ShortName = match.ShortName,
                OldUnitId = match.OldUnitId
            };
        }

    }
}