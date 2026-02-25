using MediatR;

namespace PartyManagement.Application.PartyGroup.Command.CreatePartyGroup
{
    public class CreatePartyGroupCommand : IRequest<int>
    {
        public string? PartyGroupName { get; set; }
        public int? ParentPartyGroupId { get; set; }
        public int GroupTypeId { get; set; }
        public string? Description { get; set; }
        public string? Glcode { get; set; }
        public int GlCategoryId { get; set; }
        public byte IsGroup { get; set; }
    }
}