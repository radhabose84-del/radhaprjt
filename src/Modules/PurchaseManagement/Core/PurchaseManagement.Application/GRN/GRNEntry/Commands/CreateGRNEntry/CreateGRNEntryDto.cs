namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry
{
    public class CreateGRNEntryDto
    {
        public int UnitId { get; set; }
        public int GateEntryId { get; set; }
        public int PartyId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTimeOffset InvoiceDate { get; set; }
        public string? DcNo { get; set; }
        public DateTimeOffset? DcDate { get; set; }
        public int ReceivingWarehouseId { get; set; }
        public string? Remarks { get; set; }
        public string? GrnReceivedImage { get; set; }
        public byte IsGrnGenerated { get; set; }
        public List<CreateGRNDetailsDto>? GRNDetailsDtos { get; set; }
        public class CreateGRNDetailsDto
        {
            public int PoId { get; set; }
            public int? PoSlNoLocal { get; set; }
            public int PoCategoryId { get; set; }
            public int PoMethodId { get; set; }
            public int ItemId { get; set; }
            public decimal OrderQuantity { get; set; }
            public decimal DcQuantity { get; set; }
            public decimal? UpperTolerance { get; set; }
            public decimal? LowerTolerance { get; set; }
            public decimal ReceivedQuantity { get; set; }
            public DateTimeOffset? ExpiryDate { get; set; }
            public string? BatchNumber { get; set; }
            public string? GrnDetailImage { get; set; }

        }
    }
}