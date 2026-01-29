using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.FinancialYear.Command.DeleteFinancialYear
{
    public class DeleteFinancialYearCommand :IRequest<int>
    {

          public int Id { get; set; }
        
    }
}