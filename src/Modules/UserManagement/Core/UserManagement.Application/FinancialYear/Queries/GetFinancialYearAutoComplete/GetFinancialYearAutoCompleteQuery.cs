using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.FinancialYear.Queries.GetFinancialYearAutoComplete
{
    public class GetFinancialYearAutoCompleteQuery : IRequest<List<GetFinancialYearAutoCompleteDto>>
    {
          public string? SearchTerm  { get; set; } 
        
    }
}