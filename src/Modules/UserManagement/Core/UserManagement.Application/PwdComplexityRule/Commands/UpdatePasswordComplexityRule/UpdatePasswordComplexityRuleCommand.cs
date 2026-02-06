using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.PwdComplexityRule.Queries;
using MediatR;

namespace UserManagement.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule
{
    public class UpdatePasswordComplexityRuleCommand : IRequest<bool>
    {
        
        public int Id { get; set; }
       
        public string? PwdComplexityRule  { get; set; }

        public byte IsActive { get; set; }


    }
}