using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailById
{
    public class ApprovalDatafieldByIdDto
    {
        public int Id { get; set; }
        public string JsonPath { get; set; }
        public string ValueType { get; set; }
        public string Scope { get; set; }
    }
}