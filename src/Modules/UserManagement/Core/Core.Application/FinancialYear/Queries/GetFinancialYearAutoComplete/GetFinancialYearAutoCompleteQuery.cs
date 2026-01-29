using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.FinancialYear.Queries.GetFinancialYearAutoComplete
{
    public class GetFinancialYearAutoCompleteQuery : IRequest<List<GetFinancialYearAutoCompleteDto>>
    {
          public string? SearchTerm  { get; set; } 
        
    }
}