using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExistingVendorDetails
{
    public class GetExistingVendorDetailsQuery : IRequest<ApiResponseDTO<List<GetExistingVendorDetailsDto>>>
    {
        public string? OldUnitCode { get; set; }
        public string? VendorCode { get; set; }
       
    }
}