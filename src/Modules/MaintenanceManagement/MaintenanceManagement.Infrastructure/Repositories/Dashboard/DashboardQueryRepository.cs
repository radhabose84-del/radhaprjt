#nullable disable
// Updated Repository and Supporting Code for Unified ChartDto Across Dashboards

using System.Data;
using System.Linq;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IDashboard;
using MaintenanceManagement.Application.Dashboard.CardView;
using MaintenanceManagement.Application.Dashboard.Common;
using MaintenanceManagement.Application.Dashboard.ItemConsumption;
using MaintenanceManagement.Application.Dashboard.MaintenanceHrs;
using MaintenanceManagement.Application.Dashboard.WorkOrderSummary;
using Dapper;
using MaintenanceManagement.Infrastructure.Repositories.Common;

namespace MaintenanceManagement.Infrastructure.Repositories.Dashboard
{
    public class DashboardQueryRepository : BaseQueryRepository, IDashboardQueryRepository
    {
        private readonly IDbConnection _connection;
        private readonly IDepartmentLookup _departmentLookup;
        public DashboardQueryRepository(
            IDbConnection connection,
            IDepartmentLookup departmentLookup,
            IIPAddressService ipAddressService)
            : base(ipAddressService)
        {
            _connection = connection;
            _departmentLookup = departmentLookup;
        }
        public async Task<ChartDto> WorkOrderSummaryAsync(DateTime fromDate, DateTime toDate, string departmentId, string machineGroupId)
        {
            var data = await _connection.QueryAsync<WorkOrderDashboardDto>(
                "Dashboard_Maintenance",
                new { FromDate = fromDate, ToDate = toDate, UnitId = UnitId, Type = "WorkOrderSummary", DeptId = departmentId, MachineGroupId = machineGroupId },
                commandType: CommandType.StoredProcedure);

            var months = data.Select(x => x.Month).Distinct().OrderBy(m => m).ToList();
            var series = data
                .GroupBy(d => d.StatusName)
                .Select(g => new ChartSeriesDto
                {
                    Name = g.Key,
                    Data = months.Select(m => g.FirstOrDefault(x => x.Month == m)?.Total ?? 0).ToList()
                }).ToList();

            return new ChartDto
            {
                Categories = months,
                Series = series
            };
        }

        public async Task<ChartDto> ItemConsumptionSummaryAsync(DateTime fromDate, DateTime toDate, string departmentId, string machineGroupId)
        {
            var data = await _connection.QueryAsync<ItemConsumptionDto>(
                "Dashboard_Maintenance",
                new { FromDate = fromDate, ToDate = toDate, UnitId = UnitId, Type = "ItemConsumption", DeptId = departmentId, MachineGroupId = machineGroupId },
                commandType: CommandType.StoredProcedure);

            var series = data
                .GroupBy(x => x.Item ?? "Unnamed Item")
                .Select(g => new ChartSeriesDto
                {
                    Name = g.Key,
                    Data = new List<decimal> { g.Sum(x => x.IssueQty) }
                }).ToList();

            return new ChartDto
            {
                Categories = new List<string> { "Qty" },
                Series = series
            };
        }
        public async Task<ChartDto> MaintenanceHoursDeptAsync(DateTime fromDate, DateTime toDate, string type, string departmentId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("FromDate", fromDate);
            parameters.Add("ToDate", toDate);
            parameters.Add("UnitId", UnitId);
            parameters.Add("Type", type);            
            var data = await _connection.QueryAsync<MaintenanceHrsDto>(
                "Dashboard_Maintenance",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var deptLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);

            foreach (var item in data)
            {
                if (deptLookup.TryGetValue(item.Id, out var name))
                    item.Name = $"{name}-{item.Id}";
                else
                    item.Name = $"Unknown-{item.Id}";
            }

            // Build categories
            var categories = data.Select(x => x.Name ?? "Unknown").Distinct().ToList();
            // Series: MaintenanceHrs
            var maintenanceSeries = new ChartSeriesDto
            {
                Name = "Maintenance Hrs",
                Data = categories.Select(c => data.FirstOrDefault(d => d.Name == c)?.MaintenanceHrs ?? 0).ToList()
            };

            // Series: DowntimeHrs
            var downtimeSeries = new ChartSeriesDto
            {
                Name = "Downtime Hrs",
                Data = categories.Select(c => data.FirstOrDefault(d => d.Name == c)?.DowntimeHrs ?? 0).ToList()
            };

            return new ChartDto
            {
                Categories = categories,
                Series = new List<ChartSeriesDto> { maintenanceSeries, downtimeSeries }
            };
        }

        public async Task<ChartDto> MaintenanceHoursMachineGroupAsync(DateTime fromDate, DateTime toDate, string type, string departmentId = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("FromDate", fromDate);
            parameters.Add("ToDate", toDate);
            parameters.Add("UnitId", UnitId);
            parameters.Add("Type", type);
            parameters.Add("DeptId", departmentId);

            var data = await _connection.QueryAsync<MaintenanceHrsDto>(
                "Dashboard_Maintenance",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            // Build categories
            var categories = data.Select(x => x.Name ?? "Unknown").Distinct().ToList();
            // Series: MaintenanceHrs
            var maintenanceSeries = new ChartSeriesDto
            {
                Name = "Maintenance Hrs",
                Data = categories.Select(c => data.FirstOrDefault(d => d.Name == c)?.MaintenanceHrs ?? 0).ToList()
            };

            // Series: DowntimeHrs
            var downtimeSeries = new ChartSeriesDto
            {
                Name = "Downtime Hrs",
                Data = categories.Select(c => data.FirstOrDefault(d => d.Name == c)?.DowntimeHrs ?? 0).ToList()
            };

            return new ChartDto
            {
                Categories = categories,
                Series = new List<ChartSeriesDto> { maintenanceSeries, downtimeSeries }
            };
        }
        public async Task<ChartDto> MaintenanceHoursMachineAsync(DateTime fromDate, DateTime toDate, string type, string departmentId = null, string machineGroupId = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("FromDate", fromDate);
            parameters.Add("ToDate", toDate);
            parameters.Add("UnitId", UnitId);
            parameters.Add("Type", type);
            parameters.Add("DeptId", departmentId);
            parameters.Add("MachineGroupId", machineGroupId);

            var data = await _connection.QueryAsync<MaintenanceHrsDto>(
                "Dashboard_Maintenance",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            // Build categories
            var categories = data.Select(x => x.Name ?? "Unknown").Distinct().ToList();
            // Series: MaintenanceHrs
            var maintenanceSeries = new ChartSeriesDto
            {
                Name = "Maintenance Hrs",
                Data = categories.Select(c => data.FirstOrDefault(d => d.Name == c)?.MaintenanceHrs ?? 0).ToList()
            };

            // Series: DowntimeHrs
            var downtimeSeries = new ChartSeriesDto
            {
                Name = "Downtime Hrs",
                Data = categories.Select(c => data.FirstOrDefault(d => d.Name == c)?.DowntimeHrs ?? 0).ToList()
            };

            return new ChartDto
            {
                Categories = categories,
                Series = new List<ChartSeriesDto> { maintenanceSeries, downtimeSeries }
            };
        }
        public async Task<ChartDto> ItemConsumptionDeptSummaryAsync(DateTime fromDate, DateTime toDate, string type, string departmentId, string itemCode = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("FromDate", fromDate);
            parameters.Add("ToDate", toDate);
            parameters.Add("UnitId", UnitId);
            parameters.Add("Type", type);
            parameters.Add("DeptId", departmentId);
            parameters.Add("ItemCode", itemCode);

            var data = await _connection.QueryAsync<ItemConsumptionDto>(
                "Dashboard_Maintenance",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var departments = await _departmentLookup.GetAllDepartmentAsync();
            var deptLookup = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            
            foreach (var item in data)
            {
                if (deptLookup.TryGetValue(item.Id, out var name))
                    item.Name = $"{name}-{item.Id}";
                else
                    item.Name = $"Unknown-{item.Id}";
            }

            return new ChartDto
            {
                Categories = data.Select(d => d.Name ?? "Unknown").ToList(),
                Series = new List<ChartSeriesDto>
                {
                    new ChartSeriesDto
                    {
                        Name = "Consumption",
                        Data = data.Select(d => d.IssueQty).ToList()
                    }
                }
            };
        }
        public async Task<ChartDto> ItemConsumptionMachineSummaryAsync(DateTime fromDate, DateTime toDate, string type, string departmentId, string itemCode = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("FromDate", fromDate);
            parameters.Add("ToDate", toDate);
            parameters.Add("UnitId", UnitId);
            parameters.Add("Type", type);
            parameters.Add("DeptId", departmentId);
            parameters.Add("ItemCode", itemCode);

            var data = await _connection.QueryAsync<ItemConsumptionDto>(
                "Dashboard_Maintenance",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return new ChartDto
            {
                Categories = data.Select(d => d.Name ?? "Unknown").ToList(),
                Series = new List<ChartSeriesDto>
                {
                    new ChartSeriesDto
                    {
                        Name = "Consumption",
                        Data = data.Select(d => d.IssueQty).ToList()
                    }
                }
            };
        }
        public async Task<CardViewDto> GetCardDashboardAsync(DateTime fromDate, DateTime toDate, string type, string departmentId, string machineGroupId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("FromDate", fromDate);
            parameters.Add("ToDate", toDate);
            parameters.Add("UnitId", UnitId); 
            parameters.Add("Type", type);
            parameters.Add("DeptId", departmentId);
            parameters.Add("MachineGroupId", machineGroupId);

            using var multi = await _connection.QueryMultipleAsync(
                "Dashboard_Maintenance",
                parameters,
                commandType: CommandType.StoredProcedure);

            var dto = new CardViewDto
            {
                TotalSchedules = await multi.ReadFirstAsync<int>(),
            };

            var mt = await multi.ReadFirstAsync();
            dto.MaintenanceHrs = mt.MaintenanceHrs ?? 0m;
            dto.DowntimeHrs = mt.DowntimeHrs  ?? 0m;

            dto.ConsumptionValue = await multi.ReadFirstAsync<decimal>();
            dto.OpenWorkOrder = await multi.ReadFirstAsync<int>();
            dto.InProgressWorkOrder = await multi.ReadFirstAsync<int>();
            dto.ClosedWorkOrder = await multi.ReadFirstAsync<int>();
            dto.OverDueWorkOrder = await multi.ReadFirstAsync<int>();
            dto.TopConsumptions = (await multi.ReadAsync<ConsumptionDto>()).ToList();
            return dto;
        }
    }
}
