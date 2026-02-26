using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesContact.Commands.UpdateSalesContact
{
    public class UpdateSalesContactCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string ContactName { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public int ContactTypeId { get; set; }
        public int? PartyId { get; set; }
        public string? Email { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }  // 1=Active, 0=Inactive
    }
}
