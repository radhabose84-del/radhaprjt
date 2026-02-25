namespace PartyManagement.Application.PartyGroup.Queries.GetPartyGroupAutoComplete
{
    public class PartyGroupAutoCompleteDto
    {
        public int Id { get; set; }
        public string? PartyGroupName { get; set; }
        public string? ParentPartyGroupName { get; set; }
    }
}