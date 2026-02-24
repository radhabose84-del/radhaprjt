#nullable disable
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Dtos.Lookups.Users;
using SalesManagement.Application.SalesItemPriceMaster.Commands.CreateSalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Commands.UpdateSalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Dto;

namespace SalesManagement.UnitTests.TestData
{
    /// <summary>
    /// Builder helpers for creating valid test data for SalesItemPriceMaster tests.
    /// PriceCode is immutable after creation — not included in UpdateCommand.
    /// Cross-module FKs: ItemId (IItemLookup), CurrencyId (ICurrencyLookup), PaymentTermsId (IPaymentTermLookup).
    /// Same-module FK: SalesSegmentId → Sales.SalesSegment (DB FK constraint).
    /// </summary>
    public static class SalesItemPriceMasterBuilders
    {
        private static readonly DateTimeOffset DefaultValidFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset DefaultValidTo   = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);

        // ── Create Command ────────────────────────────────────────────────────

        public static CreateSalesItemPriceMasterCommand ValidCreateCommand(
            string priceCode = "PC001",
            int itemId = 10,
            int salesSegmentId = 1,
            int paymentTermsId = 2,
            decimal exMillPrice = 100.00m,
            int currencyId = 5,
            DateTimeOffset? validFrom = null,
            DateTimeOffset? validTo = null) =>
            new CreateSalesItemPriceMasterCommand
            {
                PriceCode      = priceCode,
                ItemId         = itemId,
                SalesSegmentId = salesSegmentId,
                PaymentTermsId = paymentTermsId,
                ExMillPrice    = exMillPrice,
                CurrencyId     = currencyId,
                ValidFrom      = validFrom ?? DefaultValidFrom,
                ValidTo        = validTo   ?? DefaultValidTo
            };

        // ── Update Command ────────────────────────────────────────────────────

        public static UpdateSalesItemPriceMasterCommand ValidUpdateCommand(
            int id = 1,
            int itemId = 10,
            int salesSegmentId = 1,
            int paymentTermsId = 2,
            decimal exMillPrice = 150.00m,
            int currencyId = 5,
            DateTimeOffset? validFrom = null,
            DateTimeOffset? validTo = null,
            int isActive = 1) =>
            new UpdateSalesItemPriceMasterCommand
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

        public static SalesItemPriceMasterDto ValidDto(
            int id = 1,
            string priceCode = "PC001",
            int itemId = 10,
            int salesSegmentId = 1,
            int paymentTermsId = 2,
            decimal exMillPrice = 100.00m,
            int currencyId = 5) =>
            new SalesItemPriceMasterDto
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

        public static IReadOnlyList<SalesItemPriceMasterLookupDto> ValidLookupList() =>
            new List<SalesItemPriceMasterLookupDto>
            {
                new SalesItemPriceMasterLookupDto { Id = 1, PriceCode = "PC001", ItemName = "Item Alpha" },
                new SalesItemPriceMasterLookupDto { Id = 2, PriceCode = "PC002", ItemName = "Item Beta" }
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

        public static SalesManagement.Domain.Entities.SalesItemPriceMaster ValidEntity(int id = 1) =>
            new SalesManagement.Domain.Entities.SalesItemPriceMaster
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
