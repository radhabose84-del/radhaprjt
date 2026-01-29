namespace Contracts.Dtos.Common;  
  public sealed class PartyRefDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public int UnitId { get; set; }
}