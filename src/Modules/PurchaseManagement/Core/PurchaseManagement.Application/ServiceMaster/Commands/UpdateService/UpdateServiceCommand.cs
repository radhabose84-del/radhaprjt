using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Commands.UpdateService
{
    public class UpdateServiceCommand  : IRequest<GetServiceMasterDto>
    {
        public int Id { get; set; }

        public string ServiceDescription { get; set; } = default!;
        public int SacId { get; set; }
        public int UomId { get; set; }
        public int ServiceCategoryId { get; set; }

        // optional: change status
        public byte IsActive { get; set; }
    }
}