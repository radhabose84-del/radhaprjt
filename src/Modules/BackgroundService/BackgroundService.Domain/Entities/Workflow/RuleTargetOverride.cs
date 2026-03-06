using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;

namespace BackgroundService.Domain.Entities.Workflow
{
    public class RuleTargetOverride : BaseEntity
    {
        public int RuleId { get; set; }
        public required string Binding { get; set; }
        public required string Value { get; set; }
        public ApprovalRule Rule { get; set; } = null!;
    }
}