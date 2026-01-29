using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalRules.Commands.CreateApprovalRule
{
    public class ApprovalDatafieldDto
    {
        public string FieldKey { get; set; }
        public string JsonPath { get; set; }
        public int ValueTypeId { get; set; }
        public int ScopeId { get; set; }
    }
}