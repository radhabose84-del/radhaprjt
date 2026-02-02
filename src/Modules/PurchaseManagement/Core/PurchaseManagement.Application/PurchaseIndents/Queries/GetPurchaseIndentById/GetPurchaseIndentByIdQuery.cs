using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentById
{
    public class GetPurchaseIndentByIdQuery : IRequest<IndentByIdDto>
    {
        public int Id { get; set; }
        public int? SourceId { get; set; }
    }
}