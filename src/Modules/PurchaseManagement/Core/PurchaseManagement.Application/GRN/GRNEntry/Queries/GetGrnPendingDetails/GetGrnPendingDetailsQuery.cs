using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingDetails
{
    public class GetGrnPendingDetailsQuery : IRequest<List<GetGrnPendingDetailsDto>>
    {
        public int? GrnId { get; set; }
        public bool? IsGrnGenerated { get; set; }
        public bool? IsQcGenerated { get; set; }
        
    }
}