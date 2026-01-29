using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExistingVendorDetails
{
    public class GetExistingVendorDetailsDto
    {
        public string? VendorCode { get; set; }
        public string? VendorName { get; set; }
        public string? VendorEmail { get; set; }
        public string? VendorPhone { get; set; }

     


    }
}