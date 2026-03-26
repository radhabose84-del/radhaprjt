namespace GateEntryManagement.Application.GatePass.Dto
{
    public class CreateGatePassDetailDto
    {
        public int DocTypeId { get; set; }
        public string? DocNo { get; set; }
        public string? PartyName { get; set; }
        public string? PartyCode { get; set; }
        public DateOnly? DocDate { get; set; }
        public decimal TotalQty { get; set; }
    }
}
