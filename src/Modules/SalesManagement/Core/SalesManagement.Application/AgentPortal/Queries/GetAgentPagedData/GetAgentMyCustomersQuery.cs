using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentPortal.Dto;

namespace SalesManagement.Application.AgentPortal.Queries.GetAgentPagedData
{
    public class GetAgentMyCustomersQuery : IRequest<ApiResponseDTO<List<AgentCustomerDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class GetAgentEnquiriesQuery : IRequest<ApiResponseDTO<List<AgentEnquiryListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class GetAgentSalesOrdersQuery : IRequest<ApiResponseDTO<List<AgentSalesOrderListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class GetAgentComplaintsQuery : IRequest<ApiResponseDTO<List<AgentComplaintListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class GetAgentInvoicesQuery : IRequest<ApiResponseDTO<List<AgentInvoiceListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class GetAgentDispatchesQuery : IRequest<ApiResponseDTO<List<AgentDispatchListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class GetAgentCommissionsQuery : IRequest<ApiResponseDTO<List<AgentCommissionDto>>> { }
}
