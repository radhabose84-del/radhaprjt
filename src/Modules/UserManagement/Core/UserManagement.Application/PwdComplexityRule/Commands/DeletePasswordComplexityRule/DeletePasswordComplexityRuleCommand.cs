using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.PwdComplexityRule.Queries.GetPwdComplexityRule;
using MediatR;

namespace UserManagement.Application.PwdComplexityRule.Commands.DeletePasswordComplexityRule
{
    public class DeletePasswordComplexityRuleCommand  :IRequest<int>
    {

          public int Id { get; set; }
       
         
    }
}