using MediatR;

namespace SalesManagement.Application.SalesOrder.Queries.GetAgentCommissions
{
    public class GetAgentCommissionsQuery : IRequest<List<AgentCommissionsDto>>
    {
        public int SalesGroupId { get; set; }
        public int PaymentTermId { get; set; }
        public int AgentId { get; set; }
    }
}
