using Contracts.Common;
using Contracts.Dtos.Lookups.Party;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetVendors
{
    /// <summary>
    /// Searchable vendor (supplier) dropdown source for the External Service Request screen.
    /// Values come only from the ERP Party Master (active suppliers).
    /// </summary>
    public class GetVendorsQuery : IRequest<ApiResponseDTO<List<SupplierLookupDto>>>
    {
        public string? Term { get; set; }
    }
}
