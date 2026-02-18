using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.FinancialYear.Command.DeleteFinancialYear
{
    public class DeleteFinancialYearCommand :IRequest<int>
    {

          public int Id { get; set; }
        
    }
}