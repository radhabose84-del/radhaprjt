using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExternalRequestById;
using MaintenanceManagement.Application.Reports.MaintenanceRequestReport;


namespace MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest
{
  public interface IMaintenanceRequestQueryRepository
  {

      Task<(IEnumerable<dynamic> MaintenanceRequestList, int)> GetAllMaintenanceRequestAsync(int PageNumber, int PageSize, string? SearchTerm, DateTimeOffset? FromDate, DateTimeOffset? ToDate);
      Task<(IEnumerable<dynamic> MaintenanceRequestList, int)> GetAllMaintenanceExternalRequestAsync(int PageNumber, int PageSize, string? SearchTerm, DateTimeOffset? FromDate, DateTimeOffset? ToDate);
      // Task<MaintenanceManagement.Domain.Entities.MaintenanceRequest?> GetByIdAsync(int Id);
      Task<dynamic?> GetByIdAsync(int id);
      Task<List<GetExternalRequestByIdDto>> GetExternalRequestByIdAsync(List<int> ids);
      Task<List<MaintenanceManagement.Domain.Entities.ExistingVendorDetails>> GetVendorDetails(string OldUnitId, string? VendorCode);

      Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenancestatusAsync();
      Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceOpenstatusAsync();
      Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceRequestTypeAsync();

      Task<bool> GetWOclosedAsync(int id);
      Task<bool> GetWOclosedOrInProgressAsync(int id);

      Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceStatusDescAsync();

      Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceServiceDescAsync();
      Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceServiceLocationDescAsync();
      Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceSpareTypeDescAsync();
      Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetMaintenanceDispatchModeDescAsync();
      //Task<string> GetMachineNameAsync(int id);
      Task<(string MachineName, int DepartmentId, int Id)?> GetMachineInfoAsync(int id);
  }
}