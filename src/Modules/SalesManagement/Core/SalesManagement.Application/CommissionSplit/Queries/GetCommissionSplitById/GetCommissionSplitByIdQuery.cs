using MediatR;
using SalesManagement.Application.CommissionSplit.Dto;

namespace SalesManagement.Application.CommissionSplit.Queries.GetCommissionSplitById
{
    public class GetCommissionSplitByIdQuery : IRequest<CommissionSplitDto?>
    {
        public int Id { get; set; }
    }
}
