using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnQCCompletedDetails
{
    public class GetGrnQCCompletedDetailsQuery : IRequest<List<GetGrnQCCompletedDto>>
    {
        public int? GrnId { get; set; }
        public int ? ItemId { get; set; }
    }
}