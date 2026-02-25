namespace InventoryManagement.Domain.Common
{
    public static class MiscEnumEntity
    {
        public static class Budget_ActionType
        {
            public const string Insert = "INSERT";
            public const string Update = "UPDATE";
            public const string Delete = "DELETE";
        }
        public const string WarehouseType = "WarehouseType";
        public const string StorageType = "StorageType";
        public const string AreaType = "AreaType";
        public const string OperationType = "OperationType";
        public const string Floor = "Floor";
        public const string WarehouseAisle = "WarehouseAisle";
        public const string WarehouseRackLevel = "WarehouseRackLevel";
        public const string VariantBasedOn = "VariantBasedOn";
        public const string ItemAttribute = "VariantBasedOn";
        public const string ItemImagePath = "ImagePath";
        public const string ItemImage = "ItemImage";
        public const string sourceOfItem = "SourceofItem";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Deleted = "Deleted";
         public const string Pending = "Pending";
        public const string ApprovalStatus = "ApprovalStatus";
        public const string Consumption = "Consumption";
        public const string SubStores = "SubStore";
        public const string MaterialRequest = "Material Requisition Slip";
    }
}
