using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById
{
    public class GetPendingIndentByIdQuery : IRequest<PendingIndentByIdDto>
    {
        public int Id { get; set; }
    }
}