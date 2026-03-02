using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesOrder
{
    public interface ISalesOrderQueryRepository
    {
        Task<(List<SalesOrderHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesOrderHeaderDto?> GetByIdAsync(int id);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SalesGroupExistsAsync(int salesGroupId);
        Task<bool> SalesSegmentExistsAsync(int salesSegmentId);
        Task<bool> MiscMasterExistsAsync(int miscMasterId);
        Task<bool> UnitExistsAsync(int unitId);
        Task<bool> PartyExistsAsync(int partyId);
        Task<bool> PaymentTermExistsAsync(int paymentTermId);
        Task<bool> WarehouseExistsAsync(int warehouseId);
        Task<bool> ItemExistsAsync(int itemId);
        Task<bool> HSNExistsAsync(int hsnId);
        Task<bool> UOMExistsAsync(int uomId);
    }
}
