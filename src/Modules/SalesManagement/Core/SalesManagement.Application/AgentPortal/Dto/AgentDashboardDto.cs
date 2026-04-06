namespace SalesManagement.Application.AgentPortal.Dto
{
    public class AgentDashboardDto
    {
        public int TotalCustomers { get; set; }
        public int OpenEnquiries { get; set; }
        public int ActiveOrders { get; set; }
        public int TotalInvoices { get; set; }
        public int OpenComplaints { get; set; }
        public int TotalDispatches { get; set; }
    }
}
