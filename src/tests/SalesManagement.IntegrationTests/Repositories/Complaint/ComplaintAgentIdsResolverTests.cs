using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.Complaint;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.Complaint
{
    /// <summary>
    /// Integration tests for ComplaintQueryRepository.GetComplaintAgentIdsAsync —
    /// the Sales-only resolver behind dynamic complaint-notification recipients.
    /// Chain: ComplaintHeader → ComplaintDetail → InvoiceHeader →
    /// (LEFT) DispatchAdviceHeader → (LEFT) SalesOrderHeader,
    /// agent = COALESCE(NULLIF(InvoiceHeader.AgentId,0), SalesOrderHeader.AgentId).
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ComplaintAgentIdsResolverTests
    {
        private readonly DbFixture _fixture;

        public ComplaintAgentIdsResolverTests(DbFixture fixture) => _fixture = fixture;

        private const string DisableSalesFKs = @"
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                + ' NOCHECK CONSTRAINT ALL;' + CHAR(13)
            FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = 'Sales';
            EXEC sp_executesql @sql;";

        private ComplaintQueryRepository CreateRepo()
        {
            var party = new Mock<Contracts.Interfaces.Lookups.Party.IPartyLookup>(MockBehavior.Loose);
            var item = new Mock<Contracts.Interfaces.Lookups.Inventory.IItemLookup>(MockBehavior.Loose);
            var lot = new Mock<Contracts.Interfaces.Lookups.Production.ILotMasterLookup>(MockBehavior.Loose);
            var unit = new Mock<Contracts.Interfaces.Lookups.Users.IUnitLookup>(MockBehavior.Loose);
            var uom = new Mock<Contracts.Interfaces.Lookups.Inventory.IUOMLookup>(MockBehavior.Loose);
            var dataAccess = new Mock<Contracts.Interfaces.IDataAccessFilter>(MockBehavior.Loose);

            return new ComplaintQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                party.Object, item.Object, lot.Object, unit.Object, uom.Object,
                _fixture.IpMock.Object, dataAccess.Object);
        }

        // Seeds ComplaintHeader + one ComplaintDetail (EF, handles audit/defaults).
        private async Task<int> SeedComplaintWithDetailAsync(int invoiceHeaderId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var uniq = Guid.NewGuid().ToString("N")[..8];
            var mt = new SalesManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "CAR_MT_" + uniq,
                Description = "CAR_MT",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(mt);
            await ctx.SaveChangesAsync();

            var status = new SalesManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = "CAR_ST_" + uniq,
                Description = "Pending",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(status);
            await ctx.SaveChangesAsync();

            var ch = new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = "CAR-" + Guid.NewGuid().ToString("N")[..8],
                ComplaintDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                CustomerId = 100,
                StatusId = status.Id,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                ComplaintDetails = new List<SalesManagement.Domain.Entities.ComplaintDetail>
                {
                    new()
                    {
                        InvoiceHeaderId = invoiceHeaderId,
                        InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                        InvoiceTypeId = status.Id,
                        ItemId = 1,
                        NumberOfPacks = 1,
                        NetWeight = 1m,
                        InvoiceAmount = 1m,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }
                }
            };
            ctx.ComplaintHeader.Add(ch);
            await ctx.SaveChangesAsync();
            return ch.Id;
        }

        // Raw insert of an InvoiceHeader with explicit AgentId / DispatchAdviceId
        // (FK constraints disabled so we don't have to seed every parent).
        private async Task<int> SeedInvoiceAsync(int? agentId, int dispatchAdviceId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<int>($@"
                {DisableSalesFKs}
                INSERT INTO Sales.InvoiceHeader
                    (InvoiceNo, InvoiceDate, DispatchAdviceId, AgentId, PartyId, UnitId, FinancialYearId,
                     TotalBags, TotalWeight, TaxableValue, TotalDiscount, TotalFreight, TotalCommission,
                     Insurance, HandlingCharge, OtherCharges, TotalCharity,
                     CGST, SGST, IGST, TaxAmount, TCSPercentage, TCS,
                     RoundOff, InvoiceAmountBeforeTCS, InvoiceAmount,
                     IsActive, IsDeleted, CreatedBy)
                VALUES
                    (@No, GETDATE(), @Da, @Agent, 100, 1, 1,
                     0,0,0,0,0,0, 0,0,0,0, 0,0,0,0,0,0, 0,0,0,
                     1, 0, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { No = "INV-CAR-" + Guid.NewGuid().ToString("N")[..8], Da = dispatchAdviceId,
                      Agent = (object?)agentId ?? DBNull.Value });
        }

        private async Task<int> SeedSalesOrderAsync(int agentId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>($@"
                {DisableSalesFKs}
                INSERT INTO Sales.SalesOrderHeader
                    (SalesOrderNo, OrderDate, SalesGroupId, SalesSegmentId, EnquiryType, UnitId, PartyId,
                     PaymentTypeId, FreightTypeId, CountListId, TotalBags, TotalWeightKgs, TotalDiscountPerKg,
                     ItemValue, TotalFreight, TaxableAmount, GSTPercentage, TotalGST, TotalWithGST,
                     TCSPercentage, TotalTCS, FinalAmount, IsActive, IsDeleted, CreatedBy,
                     AgentId, StatusId, SalesOrderTypeId, OrderUnitId, SubAgentId, RevisionNumber, SplitFlag,
                     AgentCommissionId, AgentCommissionSlabId, AgentPaymentTermsId, CommissionRate,
                     CommissionValue, TotalDiscountValue)
                VALUES
                    (@No, GETDATE(), 1, 1, 30, 1, 100,
                     1, 1, 1, 0, 0, 0,
                     0, 0, 0, 0, 0, 0,
                     0, 0, 0, 1, 0, 1,
                     @Agent, 1, 1, 1, 0, 0, 1,
                     0, 0, 0, 0,
                     0, 0);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { No = "SO-CAR-" + Guid.NewGuid().ToString("N")[..8], Agent = agentId });
        }

        private async Task<int> SeedDispatchAdviceAsync(int salesOrderId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>($@"
                {DisableSalesFKs}
                INSERT INTO Sales.DispatchAdviceHeader
                    (DispatchNo, DispatchDate, StatusId, SalesOrderId, PartyId,
                     TotOrderQty, TotDispatchedQty, TotPendingQty,
                     IsActive, IsDeleted, CreatedBy, InvFlg, UnitId, DispatchTypeId, FreightId, Distance)
                VALUES
                    (@No, GETDATE(), 1, @So, 100,
                     0, 0, 0,
                     1, 0, 1, 1, 1, 1, 1, 0);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { No = "DA-CAR-" + Guid.NewGuid().ToString("N")[..8], So = salesOrderId });
        }

        [Fact]
        public async Task GetComplaintAgentIds_NoData_ReturnsEmpty()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetComplaintAgentIdsAsync(999999);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetComplaintAgentIds_InvoiceHasAgent_ReturnsInvoiceAgent()
        {
            await _fixture.ClearAllTablesAsync();
            var invoiceId = await SeedInvoiceAsync(agentId: 50, dispatchAdviceId: 0);
            var complaintId = await SeedComplaintWithDetailAsync(invoiceId);

            var result = await CreateRepo().GetComplaintAgentIdsAsync(complaintId);

            result.Should().ContainSingle().Which.Should().Be(50);
        }

        [Fact]
        public async Task GetComplaintAgentIds_InvoiceAgentZero_FallsBackToSalesOrderAgent()
        {
            await _fixture.ClearAllTablesAsync();
            var soId = await SeedSalesOrderAsync(agentId: 60);
            var daId = await SeedDispatchAdviceAsync(soId);
            var invoiceId = await SeedInvoiceAsync(agentId: 0, dispatchAdviceId: daId);
            var complaintId = await SeedComplaintWithDetailAsync(invoiceId);

            var result = await CreateRepo().GetComplaintAgentIdsAsync(complaintId);

            result.Should().ContainSingle().Which.Should().Be(60);
        }

        [Fact]
        public async Task GetComplaintAgentIds_NoAgentAnywhere_ReturnsEmpty()
        {
            await _fixture.ClearAllTablesAsync();
            var invoiceId = await SeedInvoiceAsync(agentId: 0, dispatchAdviceId: 0);
            var complaintId = await SeedComplaintWithDetailAsync(invoiceId);

            var result = await CreateRepo().GetComplaintAgentIdsAsync(complaintId);

            result.Should().BeEmpty();
        }
    }
}
