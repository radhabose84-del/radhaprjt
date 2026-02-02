using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.MRS.Queries.GetMrsEntryById
{
    public class GetMrsEntryByIdQuery : IRequest<GetMrsEntryByIdDto>
    {
        public int Id { get; set; }
    }
}