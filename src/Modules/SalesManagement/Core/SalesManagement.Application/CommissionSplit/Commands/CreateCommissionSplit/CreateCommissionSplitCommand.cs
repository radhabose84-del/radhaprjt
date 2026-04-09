using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.CommissionSplit.Commands.CreateCommissionSplit
{
    public class CreateCommissionSplitCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? SplitName { get; set; }
        public List<CommissionSplitDetailItem>? Details { get; set; }
    }

    public class CommissionSplitDetailItem
    {
        public int RoleId { get; set; }
        public int ShareTypeId { get; set; }
        public decimal ShareValue { get; set; }
    }
}
