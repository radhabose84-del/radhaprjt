using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExistingVendorDetails
{
    public class GetExistingVendorDetailsQuery : IRequest<ApiResponseDTO<List<GetExistingVendorDetailsDto>>>
    {
        public string? OldUnitCode { get; set; }
        public string? VendorCode { get; set; }
       
    }
}