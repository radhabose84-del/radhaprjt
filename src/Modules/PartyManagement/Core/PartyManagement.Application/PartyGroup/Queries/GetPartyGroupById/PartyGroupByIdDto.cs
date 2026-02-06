using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.Application.PartyGroup.Queries.GetPartyGroupById
{
    public class PartyGroupByIdDto
    {
        public int Id { get; set; }
        public string? PartyGroupName { get; set; }
        public int? ParentPartyGroupId { get; set; }
        public int GroupTypeId { get; set; }
        public string? GroupName { get; set; }
        public string? Description { get; set; }
        public byte IsGroup { get; set; }
        public string? Glcode { get; set; }
        public int GlCategoryId { get; set; }
        public Status IsActive { get; set; }
    }
}