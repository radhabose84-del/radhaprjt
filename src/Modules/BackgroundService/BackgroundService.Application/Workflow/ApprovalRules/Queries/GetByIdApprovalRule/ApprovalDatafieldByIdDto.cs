using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalRules.Queries.GetByIdApprovalRule
{
    public class ApprovalDatafieldByIdDto
    {
        public int Id { get; set; }
        public string FieldKey { get; set; }
        public string JsonPath { get; set; }
        public int ValueTypeId { get; set; }
        public int ScopeId { get; set; }
    }
}