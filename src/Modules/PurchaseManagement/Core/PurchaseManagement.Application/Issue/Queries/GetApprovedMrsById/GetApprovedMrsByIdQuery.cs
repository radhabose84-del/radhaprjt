using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.Issue.Queries.GetApprovedMrsById
{
    public class GetApprovedMrsByIdQuery  : IRequest<List<GetApprovedMrsByIdDto>>
    {
        public string? SearchPattern { get; set; }
    }
}