using Contracts.Common;
using MediatR;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Commands.UpdateComplaint
{
    public class UpdateComplaintCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; }
        public DateOnly ComplaintDate { get; set; }
        public int CustomerId { get; set; }

        // Customer Snapshot Fields
        public string? CustomerAddress { get; set; }
        public string? CustomerPIN { get; set; }
        public string? CustomerMobile { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPAN { get; set; }
        public string? CustomerGSTNo { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal TotalOS { get; set; }
        public decimal Outstanding { get; set; }
        public decimal BalanceCredit { get; set; }
        public string? Delay { get; set; }
        public string? Ledger { get; set; }

        public string? Remarks { get; set; }
        public int IsActive { get; set; }
        public List<CreateComplaintDetailDto>? Details { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
