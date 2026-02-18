using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;
using MediatR;

namespace UserManagement.Application.FinancialYear.Queries.GetFinancialYearGetById
{
    public class GetFinancialYearByIdQuery   : IRequest<List<GetFinancialYearDto>>
    { 
     
           public int Id { get; set; }
        
    }
}