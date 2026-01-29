using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace Core.Application.Entity.Queries.GetEntityBasedCompany
{
    public class GetEntityBasedCompanyQuery : IRequest<List<GetEntityBasedCompanyDto>>
    {
         public int EntityId { get; set; } 
    }
}