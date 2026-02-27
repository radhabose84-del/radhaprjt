using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Dtos.Lookups.Users;
using SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.UpdateItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;

namespace SalesManagement.UnitTests.TestData
{
    /// <summary>
    /// Builder helpers for creating valid test data for ItemPriceMaster tests.
    /// PriceCode is immutable after creation — not included in UpdateCommand.
    /// Cross-module FKs: ItemId (IItemLookup), CurrencyId (ICurrencyLookup), PaymentTermsId (IPaymentTermLookup).
    /// Same-module FK: SalesSegmentId → Sales.SalesSegment (DB FK constraint).
    /// </summary>
    public static class ItemPriceMasterBuilders
    {
        private static readonly DateOnly DefaultValidFrom = new DateOnly(2025, 1, 1);
        private static readonly DateOnly DefaultValidTo   = new DateOnly(2025, 12, 31);

        // ── Create Command ────────────────────────────────────────────────────

        public static CreateItemPriceMasterCommand ValidCreateCommand(
            string? priceCode = "PC001",
            int itemId = 10,
            int salesSegmentId = 1,
            int paymentTermsId = 2,
            decimal exMillPrice = 100.00m,
            int currencyId = 5,
            DateOnly? validFrom = null,
            DateOnly? validTo = null) =>
            new CreateItemPriceMasterCommand
            {
                PriceCode      = priceCode!,
                ItemId         = itemId,
                SalesSegmentId = salesSegmentId,
                PaymentTermsId = paymentTermsId,
                ExMillPrice    = exMillPrice,
                CurrencyId     = currencyId,
                ValidFrom      = validFrom ?? DefaultValidFrom,
                ValidTo        = validTo   ?? DefaultValidTo
            };

        // ── Update Command ────────────────────────────────────────────────────

        public static UpdateItemPriceMasterCommand ValidUpdateCommand(
            int id = 1,
            int itemId = 10,
            int salesSegmentId = 1,
            int paymentTermsId = 2,
            decimal exMillPrice = 150.00m,
            int currencyId = 5,
            DateOnly? validFrom = null,
            DateOnly? validTo = null,
            int isActive = 1) =>
            new UpdateItemPriceMasterCommand
            {
                Id             = id,
                ItemId         = itemId,
                SalesSegmentId = salesSegmentId,
                PaymentTermsId = paymentTermsId,
                ExMillPrice    = exMillPrice,
                CurrencyId     = currencyId,
                ValidFrom      = validFrom ?? DefaultValidFrom,
                ValidTo        = validTo   ?? DefaultValidTo,
                IsActive       = isActive
            };

        // ── Full DTO ──────────────────────────────────────────────────────────

        public static ItemPriceMasterDto ValidDto(
            int id = 1,
            string priceCode = "PC001",
            int itemId = 10,
            int salesSegmentId = 1,
            int paymentTermsId = 2,
            decimal exMillPrice = 100.00m,
            int currencyId = 5) =>
            new ItemPriceMasterDto
            {
                Id                     = id,
                PriceCode              = priceCode,
                ItemId                 = itemId,
                ItemCode               = "ITEM001",
                ItemName               = "Test Item",
                SalesSegmentId         = salesSegmentId,
                SalesSegmentName       = "Test Segment",
                PaymentTermsId         = paymentTermsId,
                PaymentTermsCode       = "NET30",
                PaymentTermsDescription = "Net 30 Days",
                ExMillPrice            = exMillPrice,
                CurrencyId             = currencyId,
                CurrencyCode           = "USD",
                ValidFrom              = DefaultValidFrom,
                ValidTo                = DefaultValidTo,
                IsActive               = true,
                IsDeleted              = false,
                CreatedBy              = 1,
                CreatedDate            = DateTimeOffset.UtcNow,
                CreatedByName          = "test-user"
            };

        // ── Lookup DTO List ───────────────────────────────────────────────────

        public static IReadOnlyList<ItemPriceMasterLookupDto> ValidLookupList() =>
            new List<ItemPriceMasterLookupDto>
            {
                new ItemPriceMasterLookupDto { Id = 1, PriceCode = "PC001", ItemName = "Item Alpha" },
                new ItemPriceMasterLookupDto { Id = 2, PriceCode = "PC002", ItemName = "Item Beta" }
            };

        // ── Item Lookup ───────────────────────────────────────────────────────

        public static IReadOnlyList<ItemLookupDto> ValidItemList(int itemId = 10) =>
            new List<ItemLookupDto>
            {
                new ItemLookupDto { Id = itemId, ItemCode = "ITEM001", ItemName = "Test Item" }
            };

        public static IReadOnlyList<ItemLookupDto> EmptyItemList() =>
            new List<ItemLookupDto>();

        // ── Currency Lookup ───────────────────────────────────────────────────

        public static IReadOnlyList<CurrencyLookupDto> ValidCurrencyList(int currencyId = 5) =>
            new List<CurrencyLookupDto>
            {
                new CurrencyLookupDto { CurrencyId = currencyId, Code = "USD", Name = "US Dollar" }
            };

        public static IReadOnlyList<CurrencyLookupDto> EmptyCurrencyList() =>
            new List<CurrencyLookupDto>();

        // ── Payment Term Lookup ───────────────────────────────────────────────

        public static List<PaymentTermLookupDto> ValidPaymentTermList(int id = 2) =>
            new List<PaymentTermLookupDto>
            {
                new PaymentTermLookupDto { Id = id, Code = "NET30", Description = "Net 30 Days" }
            };

        public static List<PaymentTermLookupDto> EmptyPaymentTermList() =>
            new List<PaymentTermLookupDto>();

        // ── Domain Entity ─────────────────────────────────────────────────────

        public static SalesManagement.Domain.Entities.ItemPriceMaster ValidEntity(int id = 1) =>
            new SalesManagement.Domain.Entities.ItemPriceMaster
            {
                Id             = id,
                PriceCode      = "PC001",
                ItemId         = 10,
                SalesSegmentId = 1,
                PaymentTermsId = 2,
                ExMillPrice    = 100.00m,
                CurrencyId     = 5,
                ValidFrom      = DefaultValidFrom,
                ValidTo        = DefaultValidTo,
                IsActive       = SalesManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted      = SalesManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
