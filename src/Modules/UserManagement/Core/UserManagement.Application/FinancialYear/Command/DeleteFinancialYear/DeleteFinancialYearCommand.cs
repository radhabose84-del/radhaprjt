using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.FinancialYear.Command.DeleteFinancialYear
{
    public class DeleteFinancialYearCommand :IRequest<int>
    {

          public int Id { get; set; }
        
    }
}