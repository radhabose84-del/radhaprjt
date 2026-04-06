using SalesManagement.Application.AgentPortal.Dto;

namespace SalesManagement.Application.Common.Interfaces.IAgentPortal
{
    public interface IAgentPortalQueryRepository
    {
        Task<List<int>> GetAgentCustomerIdsAsync(int agentPartyId);
        Task<AgentDashboardDto> GetDashboardAsync(int agentPartyId, List<int> customerIds);
        Task<(List<AgentCustomerDto>, int)> GetMyCustomersAsync(int agentPartyId, int pageNumber, int pageSize, string? searchTerm);
        Task<(List<AgentEnquiryListDto>, int)> GetEnquiriesAsync(List<int> customerIds, int pageNumber, int pageSize, string? searchTerm);
        Task<(List<AgentSalesOrderListDto>, int)> GetSalesOrdersAsync(List<int> customerIds, int pageNumber, int pageSize, string? searchTerm);
        Task<(List<AgentComplaintListDto>, int)> GetComplaintsAsync(List<int> customerIds, int pageNumber, int pageSize, string? searchTerm);
        Task<(List<AgentInvoiceListDto>, int)> GetInvoicesAsync(List<int> customerIds, int pageNumber, int pageSize, string? searchTerm);
        Task<(List<AgentDispatchListDto>, int)> GetDispatchesAsync(List<int> customerIds, int pageNumber, int pageSize, string? searchTerm);
        Task<List<AgentCommissionDto>> GetCommissionsAsync(int agentPartyId);
    }
}
