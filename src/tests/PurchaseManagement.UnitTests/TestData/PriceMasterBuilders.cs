using PurchaseManagement.Application.PriceMaster.Commands.Delete;
using PurchaseManagement.Domain.Entities.PriceMaster;
using DomainBase = PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class PriceMasterBuilders
    {
        public static DeletePriceMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeletePriceMasterCommand { Id = id };

        public static PriceMasterHeader ValidHeader(int id = 1) =>
            new PriceMasterHeader
            {
                Id = id,
                ItemId = 1,
                VendorId = 1,
                UomId = 1,
                ValidFrom = new DateOnly(2025, 1, 1),
                ValidTo = null,
                StatusId = 1,
                SourceFromId = 1,
                UnitId = 1,
                IsActive = DomainBase.Status.Active,
                IsDeleted = DomainBase.IsDelete.NotDeleted
            };

        public static PriceMasterDetail ValidDetail(int headerId = 1) =>
            new PriceMasterDetail
            {
                PriceMasterHeaderId = headerId,
                ScaleQtyFrom = 1,
                ScaleQtyTo = null,
                UnitPrice = 100m,
                CurrencyId = 1,
                IsActive = DomainBase.Status.Active,
                IsDeleted = DomainBase.IsDelete.NotDeleted
            };
    }
}
