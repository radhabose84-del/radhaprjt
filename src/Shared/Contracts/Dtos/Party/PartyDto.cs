namespace Contracts.Dtos.Party
{
    public class PartyContactDto
    {
        public int Id { get; set; }
        public string FirstName  { get; set; } = string.Empty;
        public string LastName   { get; set; } = string.Empty;
        public string Email      { get; set; } = string.Empty;
        public string Mobile     { get; set; } = string.Empty;
        public string ContactType{ get; set; } = string.Empty;
    }

    public class PartyDetailsDto
    {
        public int Id { get; set; }
        public string PartyCode { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string PartyPan  { get; set; } = string.Empty;
        public string PartyGst  { get; set; } = string.Empty;        

        public List<PartyContactDto> Contacts { get; set; } = new();
    }
}