using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.FinancialYear.Queries.GetFinancialYear;
using MediatR;

namespace Core.Application.FinancialYear.Queries.GetFinancialYearGetById
{
    public class GetFinancialYearByIdQuery   : IRequest<List<GetFinancialYearDto>>
    { 
     
           public int Id { get; set; }
        
    }
}