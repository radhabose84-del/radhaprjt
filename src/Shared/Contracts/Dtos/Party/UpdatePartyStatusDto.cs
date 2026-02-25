namespace Contracts.Dtos.Party
{
    public class UpdatePartyStatusDto
    {
        public int PartyId { get; set; }
        public string PartyStatus { get; set; } = default!;
        public int StatusId { get; set; }
    }
}