using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaInvoice
{
    public class UpdateProformaInvoiceCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int? StatusId { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
    }
}
