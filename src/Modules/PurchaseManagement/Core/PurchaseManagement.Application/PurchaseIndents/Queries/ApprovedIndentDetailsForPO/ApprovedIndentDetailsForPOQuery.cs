using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById;
using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO
{
    public class ApprovedIndentDetailsForPOQuery : IRequest<List<IndentForPODto>>
    {
        public int? VendorId { get; set; }
        public int? DepartmentId { get; set; }
    }

}