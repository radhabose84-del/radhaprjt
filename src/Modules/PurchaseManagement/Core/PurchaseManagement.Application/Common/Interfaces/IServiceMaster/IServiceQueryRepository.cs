using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetVendorServicePO;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.Application.ServiceMaster.Queries.GetServiceAutocomplete;

namespace PurchaseManagement.Application.Common.Interfaces.IServiceMaster
{
    public interface IServiceQueryRepository
    {
        Task<(List<GetServiceMasterDto>, int)> GetAllServiceMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<GetServiceMasterDto> GetServiceMasterByIdAsync(int Id);

        Task<bool> ExistsSimilarAsync(int sacId, int uomId, string description, int? id = null, CancellationToken ct = default);

        Task<bool> HasActiveDependenciesAsync(int serviceId, CancellationToken ct = default);

        Task<List<ServiceMasterAutoCompleteDto>> ServiceMasterAuotoComplete(string? searchTerm);

        Task<List<GetVendorServicePODto>> GetVendorApprovedServicePo(int vendorId);
        

    }
}