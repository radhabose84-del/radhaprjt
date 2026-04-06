namespace ProductionManagement.Domain.Entities
{
    public class LooseConeLedger
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public int ItemId { get; set; }
        public int LotId { get; set; }

        // DocType: 'PACK' | 'PROD' | 'REPACK' | 'CONV'
        public string? DocType { get; set; }
        public int DocNo { get; set; }
        public DateOnly DocDate { get; set; }

        public decimal LooseConeIn { get; set; }
        public decimal LooseConeOut { get; set; }

        // Running balance at the time of this entry
        public decimal AsonLooseKgs { get; set; }
    }
}
