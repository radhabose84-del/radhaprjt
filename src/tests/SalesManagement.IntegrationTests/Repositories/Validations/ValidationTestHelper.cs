using Dapper;
using Microsoft.Data.SqlClient;

namespace SalesManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Shared helper methods for seeding minimal header/detail/master rows required by
    /// validation repository integration tests. All INSERTs disable FK constraints
    /// on the Sales schema before inserting and re-enable them afterwards, so tests
    /// do not need to seed every parent table in the FK chain.
    /// </summary>
    internal static class ValidationTestHelper
    {
        private const string DisableFKs = @"
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                + ' NOCHECK CONSTRAINT ALL;' + CHAR(13)
            FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = 'Sales';
            EXEC sp_executesql @sql;";

        private const string EnableFKs = @"
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                + ' CHECK CONSTRAINT ALL;' + CHAR(13)
            FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = 'Sales';
            EXEC sp_executesql @sql;";

        // -------------------------------------------------------------------
        // Header seed helpers
        // -------------------------------------------------------------------

        public static async Task<int> SeedSalesOrderHeaderAsync(
            SqlConnection conn, int partyId = 1, int isActive = 1, int isDeleted = 0)
        {
            var uniqueNo = "SO-T-" + Guid.NewGuid().ToString("N")[..8];
            var id = await conn.ExecuteScalarAsync<int>($@"
                {DisableFKs}

                INSERT INTO Sales.SalesOrderHeader
                    (SalesOrderNo, OrderDate, SalesGroupId, EnquiryType, PartyId,
                     FreightTypeId, AgentPaymentTermsId, IsMdDiscountEnabled,
                     TotalBags, TotalWeightKgs, TotalDiscountPerKg, ItemValue,
                     TotalFreight, TaxableAmount, GSTPercentage, TotalGST,
                     TotalWithGST, TCSPercentage, TotalTCS, FinalAmount,
                     IsActive, IsDeleted, CreatedBy)
                VALUES
                    (@UniqueNo, GETDATE(), 0, 0, @PartyId,
                     0, 0, 0,
                     0, 0, 0, 0,
                     0, 0, 0, 0,
                     0, 0, 0, 0,
                     @IsActive, @IsDeleted, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { UniqueNo = uniqueNo, PartyId = partyId, IsActive = isActive, IsDeleted = isDeleted });

            await conn.ExecuteAsync(EnableFKs);
            return id;
        }

        public static async Task<int> SeedInvoiceHeaderAsync(
            SqlConnection conn, int partyId = 1, int isActive = 1, int isDeleted = 0)
        {
            var id = await conn.ExecuteScalarAsync<int>($@"
                {DisableFKs}

                INSERT INTO Sales.InvoiceHeader
                    (InvoiceDate, DispatchAdviceId, PartyId, UnitId, FinancialYearId,
                     TotalBags, TotalWeight, TaxableValue,
                     TotalDiscount, TotalFreight, TotalCommission,
                     Insurance, HandlingCharge, OtherCharges, TotalCharity,
                     CGST, SGST, IGST, TaxAmount, TCSPercentage, TCS,
                     RoundOff, InvoiceAmountBeforeTCS, InvoiceAmount,
                     IsActive, IsDeleted, CreatedBy)
                VALUES
                    (GETDATE(), 0, @PartyId, 1, 1,
                     0, 0, 0,
                     0, 0, 0,
                     0, 0, 0, 0,
                     0, 0, 0, 0, 0, 0,
                     0, 0, 0,
                     @IsActive, @IsDeleted, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { PartyId = partyId, IsActive = isActive, IsDeleted = isDeleted });

            await conn.ExecuteAsync(EnableFKs);
            return id;
        }

        public static async Task<int> SeedDeliveryChallanHeaderAsync(
            SqlConnection conn, int isActive = 1, int isDeleted = 0)
        {
            var uniqueNo = "DC-T-" + Guid.NewGuid().ToString("N")[..8];
            var id = await conn.ExecuteScalarAsync<int>($@"
                {DisableFKs}

                INSERT INTO Sales.DeliveryChallanHeader
                    (DeliveryNumber, DeliveryDate, StoHeaderId, DcTypeId, MovementTypeId,
                     FromPlantId, FromStorageLocationId, ToPlantId, ToStorageLocationId,
                     TransporterId, VehicleNumber,
                     DeliveryValue, ConsignmentValue, StatusId,
                     IsActive, IsDeleted, CreatedBy)
                VALUES
                    (@UniqueNo, GETDATE(), 0, 0, 0,
                     1, 1, 1, 1,
                     1, 'TEST-VH-01',
                     0, 0, 0,
                     @IsActive, @IsDeleted, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { UniqueNo = uniqueNo, IsActive = isActive, IsDeleted = isDeleted });

            await conn.ExecuteAsync(EnableFKs);
            return id;
        }

        public static async Task<int> SeedSalesQuotationHeaderAsync(
            SqlConnection conn, int customerId = 1, int isActive = 1, int isDeleted = 0)
        {
            var id = await conn.ExecuteScalarAsync<int>($@"
                {DisableFKs}

                INSERT INTO Sales.SalesQuotationHeader
                    (CustomerId, QuotationDate, ValidityDate,
                     PaymentTermId, DeliveryTermId,
                     FreightCharges, OtherCharges,
                     TotalBasicAmount, TotalDiscount, NetTaxableAmount,
                     TotalTax, GrandTotal,
                     IsActive, IsDeleted, CreatedBy)
                VALUES
                    (@CustomerId, GETDATE(), DATEADD(MONTH, 1, GETDATE()),
                     0, 0,
                     0, 0,
                     0, 0, 0,
                     0, 0,
                     @IsActive, @IsDeleted, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { CustomerId = customerId, IsActive = isActive, IsDeleted = isDeleted });

            await conn.ExecuteAsync(EnableFKs);
            return id;
        }

        // -------------------------------------------------------------------
        // Detail row seed helpers
        // -------------------------------------------------------------------

        /// <summary>
        /// Inserts a minimal SalesOrderDetail with all required NOT NULL columns.
        /// Specify optional overrides for the FK columns being tested.
        /// </summary>
        public static async Task SeedSalesOrderDetailAsync(
            SqlConnection conn,
            int headerId,
            int itemId = 1,
            int hsnId = 0,
            int saleUomId = 0,
            int? packTypeId = null)
        {
            await conn.ExecuteAsync($@"
                {DisableFKs}

                INSERT INTO Sales.SalesOrderDetail
                    (SalesOrderHeaderId, ItemId, HSNId, SaleUOMId, PackTypeId,
                     QtyInBags, BagWeight, TotalWeight,
                     ExMillRate, DiscountPerUnit, Freight,
                     TaxableAmount, TaxPercentage, TaxAmount,
                     TCSPercentage, TCSAmount,
                     NetAmount, NetRatePerKg,
                     ExpectedDeliveryDate, AgentCommissionPercentage,
                     DispatchedQty, PendingQty)
                VALUES
                    (@HeaderId, @ItemId, @HSNId, @SaleUOMId, @PackTypeId,
                     1, 1, 1,
                     0, 0, 0,
                     0, 0, 0,
                     0, 0,
                     0, 0,
                     GETDATE(), 0,
                     0, 0);",
                new { HeaderId = headerId, ItemId = itemId, HSNId = hsnId, SaleUOMId = saleUomId, PackTypeId = packTypeId });

            await conn.ExecuteAsync(EnableFKs);
        }

        /// <summary>
        /// Inserts a minimal InvoiceDetail with all required NOT NULL columns.
        /// </summary>
        public static async Task SeedInvoiceDetailAsync(
            SqlConnection conn,
            int headerId,
            int itemId = 1,
            int? uomId = null,
            int? lotId = null,
            int? packTypeId = null)
        {
            await conn.ExecuteAsync($@"
                {DisableFKs}

                INSERT INTO Sales.InvoiceDetail
                    (InvoiceHeaderId, ItemSno, ItemId, GstPercentage,
                     LotId, NoOfBags, BagWeight, NetWeight, RatePerKg,
                     DiscountValue, FreightValue, CommissionValue,
                     TaxableAmount, CgstPercentage, SgstPercentage, IgstPercentage,
                     CGST, SGST, IGST, TaxAmount, Charity, HandlingCharges,
                     PackTypeId, UOMId, TotalAmount)
                VALUES
                    (@HeaderId, 1, @ItemId, 0,
                     @LotId, 0, 1, 1, 0,
                     0, 0, 0,
                     0, 0, 0, 0,
                     0, 0, 0, 0, 0, 0,
                     @PackTypeId, @UOMId, 0);",
                new { HeaderId = headerId, ItemId = itemId, UOMId = uomId, LotId = lotId, PackTypeId = packTypeId });

            await conn.ExecuteAsync(EnableFKs);
        }

        /// <summary>
        /// Inserts a minimal SalesQuotationDetail with all required NOT NULL columns.
        /// </summary>
        public static async Task SeedSalesQuotationDetailAsync(
            SqlConnection conn,
            int headerId,
            int itemId = 1,
            int hsnId = 0)
        {
            await conn.ExecuteAsync($@"
                {DisableFKs}

                INSERT INTO Sales.SalesQuotationDetail
                    (SalesQuotationHeaderId, ItemId, Quantity, ExMillRate,
                     Discount, NetRate, TotalAmount,
                     HSNId, TaxPercentage, TaxAmount)
                VALUES
                    (@HeaderId, @ItemId, 1, 0,
                     0, 0, 0,
                     @HSNId, 0, 0);",
                new { HeaderId = headerId, ItemId = itemId, HSNId = hsnId });

            await conn.ExecuteAsync(EnableFKs);
        }

        /// <summary>
        /// Inserts a minimal DeliveryChallanDetail with all required NOT NULL columns.
        /// </summary>
        public static async Task SeedDeliveryChallanDetailAsync(
            SqlConnection conn,
            int headerId,
            int itemId = 1,
            int lotId = 0,
            int uomId = 0)
        {
            await conn.ExecuteAsync($@"
                {DisableFKs}

                INSERT INTO Sales.DeliveryChallanDetail
                    (DeliveryChallanHeaderId, StoDetailId, ItemId, LotId,
                     StartPackNo, EndPackNo,
                     DispatchQuantity, UOMId,
                     NetWeight, ExMillRate, LineMovementValue)
                VALUES
                    (@HeaderId, 0, @ItemId, @LotId,
                     0, 0,
                     1, @UOMId,
                     0, 0, 0);",
                new { HeaderId = headerId, ItemId = itemId, LotId = lotId, UOMId = uomId });

            await conn.ExecuteAsync(EnableFKs);
        }

        // -------------------------------------------------------------------
        // Master table seed helpers
        // -------------------------------------------------------------------

        public static async Task<int> SeedSalesSegmentAsync(
            SqlConnection conn, int currencyId = 0, int isActive = 1, int isDeleted = 0)
        {
            var id = await conn.ExecuteScalarAsync<int>($@"
                {DisableFKs}

                INSERT INTO Sales.SalesSegment
                    (SalesOrganisationId, SalesChannelId, BusinessUnitId, CurrencyId,
                     SegmentName, IsActive, IsDeleted, CreatedBy)
                VALUES (0, 0, 0, @CurrencyId, 'TS-' + LEFT(CAST(NEWID() AS VARCHAR(36)), 8),
                        @IsActive, @IsDeleted, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { CurrencyId = currencyId, IsActive = isActive, IsDeleted = isDeleted });

            await conn.ExecuteAsync(EnableFKs);
            return id;
        }

        public static async Task<int> SeedItemPriceMasterAsync(
            SqlConnection conn, int itemId = 1, int currencyId = 1, int isActive = 1, int isDeleted = 0)
        {
            var uniqueCode = "IPC-" + Guid.NewGuid().ToString("N")[..8];
            var id = await conn.ExecuteScalarAsync<int>($@"
                {DisableFKs}

                INSERT INTO Sales.ItemPriceMaster
                    (PriceCode, ItemId, SalesSegmentId, BaseRate, CurrencyId,
                     ValidFrom, ValidTo,
                     IsActive, IsDeleted, CreatedBy)
                VALUES
                    (@UniqueCode, @ItemId, 0, 100, @CurrencyId,
                     GETDATE(), DATEADD(YEAR, 1, GETDATE()),
                     @IsActive, @IsDeleted, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { UniqueCode = uniqueCode, ItemId = itemId, CurrencyId = currencyId, IsActive = isActive, IsDeleted = isDeleted });

            await conn.ExecuteAsync(EnableFKs);
            return id;
        }

        public static async Task<int> SeedSalesContactAsync(
            SqlConnection conn, int? partyId = null, int isActive = 1, int isDeleted = 0)
        {
            var id = await conn.ExecuteScalarAsync<int>($@"
                {DisableFKs}

                INSERT INTO Sales.SalesContact
                    (PartyId, ContactName, MobileNumber, ContactTypeId,
                     IsActive, IsDeleted, CreatedBy)
                VALUES (@PartyId, 'Test Contact', '9999999999', 0,
                        @IsActive, @IsDeleted, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { PartyId = partyId, IsActive = isActive, IsDeleted = isDeleted });

            await conn.ExecuteAsync(EnableFKs);
            return id;
        }
    }
}
