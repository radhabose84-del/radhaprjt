namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry
{
    public class PoValueDetailsDto
    {
        public int POId { get; set; }
        public int POCategoryId { get; set; }
        public int POMethodId { get; set; }
        public decimal? MiscCharges { get; set; }

        public decimal? Quantity { get; set; }
        public int? ItemId { get; set; }
        public decimal? UnitPrice { get; set; }

        public decimal? DiscountValue { get; set; }
        public decimal? PandFCharge { get; set; }
        public decimal? OtherCharge { get; set; }

        public decimal? FreightAmount { get; set; }
        public decimal? InsuranceAmount { get; set; }
        public decimal? CIFValue { get; set; }
        public decimal? BasicCustomDuty { get; set; }
        public decimal? SocialWelfareSurCharges { get; set; }
        public decimal? AgriInfraDevCess { get; set; }
        public decimal? AntiDumpingDuty { get; set; }
        public decimal? SafeguardDuty { get; set; }
        public decimal? HealthEducationCess { get; set; }

        public decimal? GSTPercentage { get; set; }
        public decimal? CGST { get; set; }
        public decimal? SGST { get; set; }
        public decimal? IGST { get; set; }
        public decimal? CGSTPercentage { get; set; }
        public decimal? SGSTPercentage { get; set; }
        public decimal? IGSTPercentage { get; set; }
        public int? UOMId { get; set; }
        public int? ItemSno { get; set; }
    }
}