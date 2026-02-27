using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.AgentCommissionConfig.Commands.UpdateAgentCommissionConfig
{
    public class UpdateAgentCommissionConfigCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int AgentId { get; set; }
        public int SalesSegmentId { get; set; }
        public int ItemId { get; set; }
        public int CommissionTypeId { get; set; }
        public int? UomId { get; set; }
        public decimal CommissionPercentage { get; set; }
        public int? CurrencyId { get; set; }
        public decimal? SubAgentPercentage { get; set; }
        public DateTimeOffset ValidityFrom { get; set; }
        public DateTimeOffset ValidityTo { get; set; }
        public int IsActive { get; set; }
    }
}
