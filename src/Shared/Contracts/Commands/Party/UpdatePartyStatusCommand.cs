namespace Contracts.Commands.Party
{
    public class UpdatePartyStatusCommand
    {
        public Guid CorrelationId { get; set; }
        public List<int> PartyIds { get; set; } = new();
        public string PartyStatus { get; set; } = default!;
        public int StatusId { get; set; }
        
    }
}