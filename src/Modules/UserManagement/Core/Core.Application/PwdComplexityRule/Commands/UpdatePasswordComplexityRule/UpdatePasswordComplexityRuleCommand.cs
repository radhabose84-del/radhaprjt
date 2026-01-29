using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.PwdComplexityRule.Queries;
using MediatR;

namespace Core.Application.PasswordComplexityRule.Commands.UpdatePasswordComplexityRule
{
    public class UpdatePasswordComplexityRuleCommand : IRequest<bool>
    {
        
        public int Id { get; set; }
       
        public string? PwdComplexityRule  { get; set; }

        public byte IsActive { get; set; }


    }
}