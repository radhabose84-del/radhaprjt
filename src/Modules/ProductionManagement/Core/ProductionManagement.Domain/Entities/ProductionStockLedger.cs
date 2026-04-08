namespace ProductionManagement.Domain.Entities
{
    /// <summary>
    /// Itemwise / Lotwise closing stock register.
    /// One row per (UnitId, ItemId, LotId, DocDate) — combines PACK + REPACK data.
    /// Running balance of loose kgs and cumulative pack kgs / bags.
    /// </summary>
    public class ProductionStockLedger
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public int ItemId { get; set; }
        public int LotId { get; set; }

        public DateOnly DocDate { get; set; }

        // Opening
        public decimal OpeningLooseKgs { get; set; }

        // Arrivals / Production
        public decimal ProdKgs { get; set; }           // New material produced
        public decimal TotalProdKgs { get; set; }      // ProdKgs + OpeningLooseKgs

        // Packing
        public int PackTypeId { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }         // TotalBags × NetWeightPerPack

        // Repacking
        public int BagsRepacked { get; set; }
        public decimal RepackKgs { get; set; }

        // Closing
        public decimal ClosingLooseKgs { get; set; }   // TotalProd − NetWeight
        public decimal ClosingPackKgs { get; set; }     // Running cumulative pack kgs
        public int ClosingBags { get; set; }            // Running cumulative bags (PACK only)
        public bool StockClosing { get; set; }          // true = closing entry for the date
    }
}
