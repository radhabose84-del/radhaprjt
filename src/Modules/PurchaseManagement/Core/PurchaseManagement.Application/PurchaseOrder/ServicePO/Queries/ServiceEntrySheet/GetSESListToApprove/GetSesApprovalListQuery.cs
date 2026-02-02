using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetSESListToApprove
{
    public class GetSesApprovalListQuery    : IRequest<List<SesApprovalListDto>>
    {
        // optional filters
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public int? VendorId { get; set; }
    }
}