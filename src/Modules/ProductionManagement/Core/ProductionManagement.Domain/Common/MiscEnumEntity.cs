namespace ProductionManagement.Domain.Common
{
    public static class MiscEnumEntity
    {
        public const string ProductionImage = "ProductionImage";
        public const string Approved = "Approved";

        // Lot
        public const string LotType = "LOT_TYPE";
        public const string LotStatus = "LOT_STATUS";

        // Quality Status
        public const string QualityStatus = "QualityStatus";
        public const string Packed = "Packed";

        // Stock Status
        public const string Deleted = "Deleted";

        // Document Sequence - Transaction Types
        public const string TransactionTypePackMaster = "PackMaster";
        public const string TransactionTypeRePackMaster = "RePackEntry";
        public const string TransactionTypeYarnConversion = "YarnConversionEntry";
        public const string ModuleSales = "Sales";
    }
}
