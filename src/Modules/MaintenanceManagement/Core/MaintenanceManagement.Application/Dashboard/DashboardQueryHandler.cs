using MaintenanceManagement.Application.Common.Interfaces.IDashboard;
using MaintenanceManagement.Application.Dashboard.Common;
using MaintenanceManagement.Application.Dashboard.DashboardQuery;
using MediatR;

public class DashboardQueryHandler : IRequestHandler<DashboardQuery, ChartDto>
{
    private readonly IDashboardQueryRepository _repository;

    public DashboardQueryHandler(IDashboardQueryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ChartDto> Handle(DashboardQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Type))
            throw new ArgumentException("Type is required. Valid values: 'workOrderSummary', 'itemConsumption', 'maintenanceHrs-dept', 'maintenanceHrs-machineGroup', 'maintenanceHrs-machine','card-dashboard'");

        return request.Type switch
        {
            "workOrderSummary" => await _repository.WorkOrderSummaryAsync(request.FromDate, request.ToDate, request.DepartmentId, request.MachineGroupId),
            "itemConsumption" => await _repository.ItemConsumptionSummaryAsync(request.FromDate, request.ToDate, request.DepartmentId, request.MachineGroupId),
            "itemConsumption-dept" => await _repository.ItemConsumptionDeptSummaryAsync(request.FromDate, request.ToDate,request.Type,request.DepartmentId,request.ItemCode),
            "itemConsumption-machineGroup" => await _repository.ItemConsumptionMachineSummaryAsync(request.FromDate, request.ToDate,request.Type,request.DepartmentId,request.ItemCode),
            "maintenanceHrs-dept" => await _repository.MaintenanceHoursDeptAsync(request.FromDate, request.ToDate,request.Type,request.DepartmentId),
            "maintenanceHrs-machineGroup" => await _repository.MaintenanceHoursMachineGroupAsync(request.FromDate, request.ToDate,request.Type,request.DepartmentId),
            "maintenanceHrs-machine" => await _repository.MaintenanceHoursMachineAsync(request.FromDate, request.ToDate,request.Type, request.DepartmentId, request.MachineGroupId),            
            _ => throw new ArgumentException("Invalid type.")
        };
    }
}
