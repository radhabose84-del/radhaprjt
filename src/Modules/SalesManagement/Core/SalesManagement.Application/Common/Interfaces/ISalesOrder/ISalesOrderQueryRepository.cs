using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetAgentCommissions;
using SalesManagement.Application.SalesOrder.Queries.GetDiscountsBySalesGroup;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrder;

namespace SalesManagement.Application.Common.Interfaces.ISalesOrder
{
    public interface ISalesOrderQueryRepository
    {
        Task<(List<SalesOrderHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, DateOnly? orderDateFrom = null, DateOnly? orderDateTo = null, string? partyName = null, string? statusName = null);
        Task<(List<PendingSalesOrderDto>, int)> GetPendingSalesOrderAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesOrderHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<SalesOrderLookupDto>> AutocompleteAsync(string term, CancellationToken ct, bool proformaFilter = false);
        Task<List<DiscountsBySalesGroupDto>> GetDiscountsBySalesGroupAsync(int salesGroupId, int slabTypeId, int paymentTermId, CancellationToken ct);
        Task<List<AgentCommissionsDto>> GetAgentCommissionsAsync(int salesGroupId, int paymentTermId, int agentId, CancellationToken ct);
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
        Task<bool> SalesQuotationHeaderExistsAsync(int id);
        Task<bool> PackTypeExistsAsync(int packTypeId);
        Task<bool> AgentExistsAsync(int agentId);
        Task<bool> SubAgentExistsAsync(int subAgentId);
        Task<bool> HasDispatchAdviceAsync(int salesOrderHeaderId);
        Task<bool> DiscountMasterExistsAsync(int discountMasterId);
    }
}
