using PartyManagement.Domain.Common;

namespace PartyManagement.Domain.Entities
{
    public class PartyGroup : BaseEntity
    {
        public string? PartyGroupName { get; set; }
        public int? ParentPartyGroupId { get; set; }
        public PartyGroup? ParentPartyGroup { get; set; }
        public int GroupTypeId { get; set; } // Foreign key to MiscMaster
        public MiscMaster GroupType { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsGroup { get; set; }
        public string? Glcode { get; set; }
        public int GlCategoryId { get; set; }
        public MiscMaster GlCategory { get; set; } = null!;
        public ICollection<PartyGroup>? ChildPartyGroups { get; set; }
        public ICollection<PartyType>? PartyTypeGroups { get; set; }

    }
}