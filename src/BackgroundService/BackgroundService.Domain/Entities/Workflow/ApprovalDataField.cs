using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.Domain.Entities.Workflow
{
    public class ApprovalDataField : BaseEntity
    {
        public  string FieldKey { get; set; }
        public  string JsonPath { get; set; }
        public  int ValueTypeId { get; set; }
        public MiscMaster ValueType { get; set; }
        public int ScopeId { get; set; }
        public MiscMaster Scope { get; set; }
        public ICollection<ApprovalRuleCondition> Conditions { get; set; }
    }
}