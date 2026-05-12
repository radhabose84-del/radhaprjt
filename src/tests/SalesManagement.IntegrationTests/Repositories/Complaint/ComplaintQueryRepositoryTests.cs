using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.Complaint;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.Complaint
{
    [Collection("DatabaseCollection")]
    public sealed class ComplaintQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ComplaintQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ---------------------------------------------------------------------------
        // Factory helpers
        // ---------------------------------------------------------------------------

        private ComplaintQueryRepository CreateRepo(
            Mock<IPartyLookup>? partyLookup = null,
            Mock<IItemLookup>? itemLookup = null,
            Mock<ILotMasterLookup>? lotLookup = null,
            Mock<IUnitLookup>? unitLookup = null,
            Mock<IUOMLookup>? uomLookup = null,
            Mock<IIPAddressService>? ipService = null,
            Mock<IDataAccessFilter>? dataAccessFilter = null)
        {
            partyLookup ??= BuildDefaultPartyLookup();
            itemLookup ??= BuildDefaultItemLookup();
            lotLookup ??= BuildDefaultLotLookup();
            unitLookup ??= BuildDefaultUnitLookup();
            uomLookup ??= BuildDefaultUomLookup();
            ipService ??= BuildDefaultIpService();
            dataAccessFilter ??= BuildUnrestrictedDataAccessFilter();

            return new ComplaintQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                partyLookup.Object,
                itemLookup.Object,
                lotLookup.Object,
                unitLookup.Object,
                uomLookup.Object,
                ipService.Object,
                dataAccessFilter.Object);
        }

        private static Mock<IPartyLookup> BuildDefaultPartyLookup()
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                        new PartyLookupDto { Id = id, PartyName = "Party " + id }).ToList());
            mock.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) =>
                    new PartyLookupDto { Id = id, PartyName = "Party " + id });
            return mock;
        }

        private static Mock<IItemLookup> BuildDefaultItemLookup()
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<ItemLookupDto>)ids.Select(id =>
                        new ItemLookupDto { Id = id, ItemCode = "I" + id, ItemName = "Item " + id }).ToList());
            return mock;
        }

        private static Mock<ILotMasterLookup> BuildDefaultLotLookup()
        {
            var mock = new Mock<ILotMasterLookup>(MockBehavior.Loose);
            mock.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<LotMasterLookupDto>)new List<LotMasterLookupDto>());
            return mock;
        }

        private static Mock<IUnitLookup> BuildDefaultUnitLookup()
        {
            var mock = new Mock<IUnitLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UnitLookupDto>)new List<UnitLookupDto>());
            return mock;
        }

        private static Mock<IUOMLookup> BuildDefaultUomLookup()
        {
            var mock = new Mock<IUOMLookup>(MockBehavior.Loose);
            return mock;
        }

        private static Mock<IIPAddressService> BuildDefaultIpService()
        {
            var mock = new Mock<IIPAddressService>(MockBehavior.Loose);
            mock.Setup(x => x.GetUserId()).Returns(1);
            mock.Setup(x => x.GetUserName()).Returns("test-user");
            mock.Setup(x => x.GetCompanyId()).Returns(1);
            mock.Setup(x => x.GetUnitId()).Returns(1);
            mock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            return mock;
        }

        private static Mock<IDataAccessFilter> BuildUnrestrictedDataAccessFilter()
        {
            var mock = new Mock<IDataAccessFilter>(MockBehavior.Loose);
            mock.Setup(d => d.GetContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(DataAccessContext.Unrestricted);
            return mock;
        }

        // ---------------------------------------------------------------------------
        // Seeding helpers
        // ---------------------------------------------------------------------------

        private async Task<int> EnsureMiscTypeAsync(ApplicationDbContext ctx, string code = "CQQ_MT")
        {
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == code);
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Complaint Test Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            return mt.Id;
        }

        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx, int miscTypeId, string code)
        {
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = code,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        /// <summary>
        /// Seeds a ComplaintHeader row and returns (statusId, complaintId).
        /// </summary>
        private async Task<(int statusId, int complaintId)> SeedComplaintAsync(
            string complaintNumber = "CQQ_C001",
            int customerId = 100,
            IsDelete deleted = IsDelete.NotDeleted,
            Status active = Status.Active,
            string? remarks = "test-remarks")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CQQ_STATUS");

            var entity = new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = complaintNumber,
                ComplaintDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                CustomerId = customerId,
                StatusId = statusId,
                Remarks = remarks,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.ComplaintHeader.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return (statusId, entity.Id);
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // ---------------------------------------------------------------------------
        // GetAllAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_GA1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_ALPHA");
            await SeedComplaintAsync("CQQ_BETA");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "CQQ_ALPHA", null);

            rows.Should().HaveCount(1);
            rows[0].ComplaintNumber.Should().Be("CQQ_ALPHA");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CustomerName_Via_Lookup()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_CUST", customerId: 42);

            var partyMock = new Mock<IPartyLookup>(MockBehavior.Loose);
            partyMock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                        new PartyLookupDto { Id = id, PartyName = "Acme Corp" }).ToList());

            var (rows, _) = await CreateRepo(partyLookup: partyMock).GetAllAsync(1, 10, null, null);

            rows.Should().HaveCount(1);
            rows[0].CustomerName.Should().Be("Acme Corp");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Pagination()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_PG1");
            await SeedComplaintAsync("CQQ_PG2");
            await SeedComplaintAsync("CQQ_PG3");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 2, null, null);

            rows.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // ---------------------------------------------------------------------------
        // GetByIdAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearAsync();
            var (_, id) = await SeedComplaintAsync("CQQ_GBI1", customerId: 55);

            var dto = await CreateRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.ComplaintNumber.Should().Be("CQQ_GBI1");
            dto.CustomerId.Should().Be(55);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var (_, id) = await SeedComplaintAsync("CQQ_GBI_DEL", deleted: IsDelete.Deleted);

            var dto = await CreateRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExistent_Id()
        {
            var dto = await CreateRepo().GetByIdAsync(9999999);

            dto.Should().BeNull();
        }

        // ---------------------------------------------------------------------------
        // NotFoundAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearAsync();
            var (_, id) = await SeedComplaintAsync("CQQ_NF1");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_SoftDeleted()
        {
            await ClearAsync();
            var (_, id) = await SeedComplaintAsync("CQQ_NF_DEL", deleted: IsDelete.Deleted);

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ---------------------------------------------------------------------------
        // CustomerExistsAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task CustomerExistsAsync_Should_Return_True_When_Lookup_Returns_Party()
        {
            // CustomerExistsAsync delegates entirely to IPartyLookup.GetByIdAsync
            var result = await CreateRepo().CustomerExistsAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CustomerExistsAsync_Should_Return_False_When_Lookup_Returns_Null()
        {
            var partyMock = new Mock<IPartyLookup>(MockBehavior.Loose);
            partyMock.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyLookupDto?)null);

            var result = await CreateRepo(partyLookup: partyMock).CustomerExistsAsync(999);

            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // AutocompleteAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Records()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_AC1", active: Status.Active);

            var results = await CreateRepo().AutocompleteAsync("CQQ_AC", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].ComplaintNumber.Should().Be("CQQ_AC1");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_INAC", active: Status.Inactive);

            var results = await CreateRepo().AutocompleteAsync("CQQ_INAC", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // ---------------------------------------------------------------------------
        // IsReadyForResolutionAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task IsReadyForResolutionAsync_Should_Return_False_When_No_QCReview()
        {
            await ClearAsync();
            var (_, id) = await SeedComplaintAsync("CQQ_RES1");

            var result = await CreateRepo().IsReadyForResolutionAsync(id);

            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // SearchInvoicesAsync — NetWeight output must come from id.BagWeight column
        // (verifies the column-alias swap from id.Quantity → id.BagWeight)
        // ---------------------------------------------------------------------------

        private const string DisableSalesFKs = @"
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                + ' NOCHECK CONSTRAINT ALL;' + CHAR(13)
            FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = 'Sales';
            EXEC sp_executesql @sql;";

        private const string EnableSalesFKs = @"
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                + ' CHECK CONSTRAINT ALL;' + CHAR(13)
            FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = 'Sales';
            EXEC sp_executesql @sql;";

        /// <summary>
        /// Seeds one InvoiceHeader + one InvoiceDetail with distinct BagWeight and NetWeight
        /// values so the test can tell which column ends up as the NetWeight DTO field.
        /// Uses raw SQL with FK constraints disabled to avoid seeding the whole parent chain
        /// (DispatchAdviceHeader, SalesOrderHeader, etc.).
        /// </summary>
        private async Task<(int invoiceHeaderId, int invoiceDetailId)> SeedInvoiceWithBagWeightAsync(
            int partyId, decimal bagWeight, decimal netWeight, int unitId = 1)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var invoiceNo = "INV-CQQ-" + Guid.NewGuid().ToString("N")[..8];

            var invoiceHeaderId = await conn.ExecuteScalarAsync<int>($@"
                {DisableSalesFKs}

                INSERT INTO Sales.InvoiceHeader
                    (InvoiceNo, InvoiceDate, DispatchAdviceId, PartyId, UnitId, FinancialYearId,
                     TotalBags, TotalWeight, TaxableValue, TotalDiscount, TotalFreight, TotalCommission,
                     Insurance, HandlingCharge, OtherCharges, TotalCharity,
                     CGST, SGST, IGST, TaxAmount, TCSPercentage, TCS,
                     RoundOff, InvoiceAmountBeforeTCS, InvoiceAmount,
                     IsActive, IsDeleted, CreatedBy)
                VALUES
                    (@InvoiceNo, GETDATE(), 0, @PartyId, @UnitId, 1,
                     0, 0, 0, 0, 0, 0,
                     0, 0, 0, 0,
                     0, 0, 0, 0, 0, 0,
                     0, 0, 0,
                     1, 0, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { InvoiceNo = invoiceNo, PartyId = partyId, UnitId = unitId });

            var invoiceDetailId = await conn.ExecuteScalarAsync<int>($@"
                INSERT INTO Sales.InvoiceDetail
                    (InvoiceHeaderId, ItemSno, ItemId, GstPercentage,
                     LotId, NoOfBags, BagWeight, NetWeight, RatePerKg,
                     DiscountValue, FreightValue, CommissionValue,
                     TaxableAmount, CgstPercentage, SgstPercentage, IgstPercentage,
                     CGST, SGST, IGST, TaxAmount, Charity, HandlingCharges,
                     PackTypeId, UOMId, TotalAmount)
                VALUES
                    (@HeaderId, 1, 1, 0,
                     NULL, 2, @BagWeight, @NetWeight, 100,
                     0, 0, 0,
                     0, 0, 0, 0,
                     0, 0, 0, 0, 0, 0,
                     NULL, 1, 200);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
                {EnableSalesFKs}",
                new { HeaderId = invoiceHeaderId, BagWeight = bagWeight, NetWeight = netWeight });

            return (invoiceHeaderId, invoiceDetailId);
        }

        private async Task ClearInvoiceTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            // Ensure Finance.TransactionTypeMaster stub exists — SearchInvoicesAsync LEFT JOINs it.
            // The table is created lazily here to avoid polluting the shared DbFixture.
            await conn.ExecuteAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Finance')
                    EXEC('CREATE SCHEMA Finance');

                IF NOT EXISTS (
                    SELECT 1 FROM sys.tables t
                    JOIN sys.schemas s ON t.schema_id = s.schema_id
                    WHERE s.name = 'Finance' AND t.name = 'TransactionTypeMaster')
                BEGIN
                    CREATE TABLE [Finance].[TransactionTypeMaster] (
                        Id          INT IDENTITY(1,1) PRIMARY KEY,
                        TypeName    NVARCHAR(100) NULL,
                        ShortName   NVARCHAR(20)  NULL,
                        IsActive    BIT           NOT NULL DEFAULT 1,
                        IsDeleted   BIT           NOT NULL DEFAULT 0
                    );
                END

                DELETE FROM Sales.InvoiceDetail;
                DELETE FROM Sales.InvoiceHeader;");
        }

        /// <summary>
        /// UOM lookup that returns an empty list so the repository's ToDictionary call doesn't throw.
        /// </summary>
        private static Mock<IUOMLookup> BuildEmptyUomLookup()
        {
            var mock = new Mock<IUOMLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UOMLookupDto>)new List<UOMLookupDto>());
            return mock;
        }

        [Fact]
        public async Task SearchInvoicesAsync_Should_Return_NetWeight_Sourced_From_BagWeight_Column()
        {
            // Arrange: seed one invoice line where BagWeight differs from NetWeight,
            // so the assertion can tell which column feeds the output.
            const int partyId = 88888;
            const decimal bagWeight = 7.5m;
            const decimal netWeightColumn = 99m;

            await ClearInvoiceTablesAsync();
            await SeedInvoiceWithBagWeightAsync(partyId, bagWeight, netWeightColumn);

            // Act
            var (rows, total) = await CreateRepo(uomLookup: BuildEmptyUomLookup())
                .SearchInvoicesAsync(partyId, searchTerm: null, lastOneYear: false, pageNumber: 1, pageSize: 10);

            // Assert: the DTO's NetWeight field carries the BagWeight value, not the NetWeight column.
            total.Should().Be(1);
            rows.Should().HaveCount(1);
            rows[0].NetWeight.Should().Be(bagWeight, "SearchInvoicesAsync projects id.BagWeight AS NetWeight");
            rows[0].NetWeight.Should().NotBe(netWeightColumn, "the output must NOT come from the renamed Quantity/NetWeight column");
        }

        [Fact]
        public async Task SearchInvoicesAsync_Should_Scope_By_PartyId()
        {
            const int targetParty = 88881;
            const int otherParty = 88882;

            await ClearInvoiceTablesAsync();
            await SeedInvoiceWithBagWeightAsync(targetParty, bagWeight: 1m, netWeight: 1m);
            await SeedInvoiceWithBagWeightAsync(otherParty, bagWeight: 2m, netWeight: 2m);

            var (rows, total) = await CreateRepo(uomLookup: BuildEmptyUomLookup())
                .SearchInvoicesAsync(targetParty, searchTerm: null, lastOneYear: false, pageNumber: 1, pageSize: 10);

            total.Should().Be(1);
            rows.Should().HaveCount(1);
            rows[0].NetWeight.Should().Be(1m);
        }

        [Fact]
        public async Task GetInvoiceLineDetailsAsync_Should_Return_Quantity_Sourced_From_NetWeight_Column()
        {
            // Repro for production SQL207 ('Invalid column name Quantity'): the table has
            // no Quantity column — the DTO's Quantity field must come from id.NetWeight.
            const int partyId = 88899;
            const decimal bagWeightColumn = 7.5m;
            const decimal netWeightColumn = 42m;

            await ClearInvoiceTablesAsync();
            var (invoiceHeaderId, _) = await SeedInvoiceWithBagWeightAsync(partyId, bagWeightColumn, netWeightColumn);

            var lines = await CreateRepo(uomLookup: BuildEmptyUomLookup())
                .GetInvoiceLineDetailsAsync(invoiceHeaderId);

            lines.Should().HaveCount(1);
            lines[0].Quantity.Should().Be(netWeightColumn,
                "GetInvoiceLineDetailsAsync projects id.NetWeight AS Quantity");
            lines[0].Quantity.Should().NotBe(bagWeightColumn,
                "Quantity output must NOT come from id.BagWeight in this query");
        }

        // ---------------------------------------------------------------------------
        // IsComplaintFinalizedAsync — used by UpdateComplaintCommandValidator to
        // block edits on QC Accepted / Closed complaints. SQL uses
        // UPPER(LTRIM(RTRIM(Description))) IN ('QC ACCEPTED', 'CLOSED').
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Seeds a complaint whose StatusId points to a MiscMaster row with the supplied
        /// Description (e.g., "QC Accepted", "Closed", "Pending"). Returns the new complaint Id.
        /// </summary>
        private async Task<int> SeedComplaintWithStatusDescriptionAsync(string description, string complaintNumber)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);

            // Insert a MiscMaster row with the specific Description we want to match
            var misc = new SalesManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId  = mtId,
                Code        = "FINALIZE_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                Description = description,
                IsActive    = Status.Active,
                IsDeleted   = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(misc);
            await ctx.SaveChangesAsync();

            var complaint = new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = complaintNumber,
                ComplaintDate   = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                CustomerId      = 100,
                StatusId        = misc.Id,
                IsActive        = Status.Active,
                IsDeleted       = IsDelete.NotDeleted
            };
            await ctx.ComplaintHeader.AddAsync(complaint);
            await ctx.SaveChangesAsync();
            return complaint.Id;
        }

        [Fact]
        public async Task IsComplaintFinalizedAsync_QCAcceptedStatus_ReturnsTrue()
        {
            await ClearAsync();
            var id = await SeedComplaintWithStatusDescriptionAsync("QC Accepted", "CQQ_FIN_QC");

            var result = await CreateRepo().IsComplaintFinalizedAsync(id);

            result.Should().BeTrue("complaint with status 'QC Accepted' must be flagged as finalized");
        }

        [Fact]
        public async Task IsComplaintFinalizedAsync_ClosedStatus_ReturnsTrue()
        {
            await ClearAsync();
            var id = await SeedComplaintWithStatusDescriptionAsync("Closed", "CQQ_FIN_CL");

            var result = await CreateRepo().IsComplaintFinalizedAsync(id);

            result.Should().BeTrue("complaint with status 'Closed' must be flagged as finalized");
        }

        [Fact]
        public async Task IsComplaintFinalizedAsync_CaseInsensitive_Description_ReturnsTrue()
        {
            await ClearAsync();
            // SQL upper-cases before comparison — lowercase Description should still match
            var id = await SeedComplaintWithStatusDescriptionAsync("qc accepted", "CQQ_FIN_LC");

            var result = await CreateRepo().IsComplaintFinalizedAsync(id);

            result.Should().BeTrue("the comparison is UPPER-cased, so case differences must not break the rule");
        }

        [Fact]
        public async Task IsComplaintFinalizedAsync_WhitespaceAroundDescription_ReturnsTrue()
        {
            await ClearAsync();
            // SQL trims leading/trailing whitespace before comparison
            var id = await SeedComplaintWithStatusDescriptionAsync("  Closed  ", "CQQ_FIN_TRIM");

            var result = await CreateRepo().IsComplaintFinalizedAsync(id);

            result.Should().BeTrue("the comparison trims whitespace, so padded values must still match");
        }

        [Fact]
        public async Task IsComplaintFinalizedAsync_OpenStatus_ReturnsFalse()
        {
            await ClearAsync();
            var id = await SeedComplaintWithStatusDescriptionAsync("Pending", "CQQ_OPEN");

            var result = await CreateRepo().IsComplaintFinalizedAsync(id);

            result.Should().BeFalse("complaint with status 'Pending' is mutable — edits must NOT be blocked");
        }

        [Fact]
        public async Task IsComplaintFinalizedAsync_SoftDeletedComplaint_ReturnsFalse()
        {
            await ClearAsync();
            // Seed a complaint then soft-delete it; the rule must only fire on live rows
            var id = await SeedComplaintWithStatusDescriptionAsync("QC Accepted", "CQQ_DEL_FIN");

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var ch = await ctx.ComplaintHeader.FirstAsync(c => c.Id == id);
                ch.IsDeleted = IsDelete.Deleted;
                await ctx.SaveChangesAsync();
            }

            var result = await CreateRepo().IsComplaintFinalizedAsync(id);

            result.Should().BeFalse("soft-deleted complaint should be unreachable; rule must not match it");
        }

        [Fact]
        public async Task IsComplaintFinalizedAsync_NonExistentId_ReturnsFalse()
        {
            await ClearAsync();

            var result = await CreateRepo().IsComplaintFinalizedAsync(int.MaxValue);

            result.Should().BeFalse("an Id that doesn't exist cannot be finalized");
        }

        // ---------------------------------------------------------------------------
        // Pull-from-Invoice unit scoping — invoices raised for the same customer
        // across multiple units must only surface for the user's current unit.
        // Regression guard for the cross-unit invoice listing bug.
        // ---------------------------------------------------------------------------

        private static Mock<IIPAddressService> BuildIpServiceWithUnit(int unitId)
        {
            var mock = new Mock<IIPAddressService>(MockBehavior.Loose);
            mock.Setup(x => x.GetUserId()).Returns(1);
            mock.Setup(x => x.GetUserName()).Returns("test-user");
            mock.Setup(x => x.GetCompanyId()).Returns(1);
            mock.Setup(x => x.GetUnitId()).Returns(unitId);
            mock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            return mock;
        }

        [Fact]
        public async Task SearchInvoicesAsync_Should_Scope_By_Current_UnitId()
        {
            const int partyId = 77771;
            const int currentUnit = 1;
            const int otherUnit = 99;

            await ClearInvoiceTablesAsync();
            await SeedInvoiceWithBagWeightAsync(partyId, bagWeight: 1m, netWeight: 1m, unitId: currentUnit);
            await SeedInvoiceWithBagWeightAsync(partyId, bagWeight: 2m, netWeight: 2m, unitId: otherUnit);

            var (rows, total) = await CreateRepo(
                    uomLookup: BuildEmptyUomLookup(),
                    ipService: BuildIpServiceWithUnit(currentUnit))
                .SearchInvoicesAsync(partyId, searchTerm: null, lastOneYear: false, pageNumber: 1, pageSize: 10);

            total.Should().Be(1, "only the current unit's invoice should be returned");
            rows.Should().HaveCount(1);
            rows[0].UnitId.Should().Be(currentUnit);
        }

        [Fact]
        public async Task SearchInvoicesAsync_Should_Return_Empty_When_No_Invoice_Matches_Current_Unit()
        {
            const int partyId = 77772;

            await ClearInvoiceTablesAsync();
            await SeedInvoiceWithBagWeightAsync(partyId, bagWeight: 1m, netWeight: 1m, unitId: 50);

            var (rows, total) = await CreateRepo(
                    uomLookup: BuildEmptyUomLookup(),
                    ipService: BuildIpServiceWithUnit(1))
                .SearchInvoicesAsync(partyId, searchTerm: null, lastOneYear: false, pageNumber: 1, pageSize: 10);

            total.Should().Be(0);
            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCustomerInvoicesAsync_Should_Scope_By_Current_UnitId()
        {
            const int customerId = 77773;
            const int currentUnit = 1;
            const int otherUnit = 99;

            await ClearInvoiceTablesAsync();
            await SeedInvoiceWithBagWeightAsync(customerId, bagWeight: 1m, netWeight: 1m, unitId: currentUnit);
            await SeedInvoiceWithBagWeightAsync(customerId, bagWeight: 2m, netWeight: 2m, unitId: otherUnit);

            var rows = await CreateRepo(
                    uomLookup: BuildEmptyUomLookup(),
                    ipService: BuildIpServiceWithUnit(currentUnit))
                .GetCustomerInvoicesAsync(customerId);

            rows.Should().HaveCount(1, "only the current unit's invoice should be returned");
        }

        [Fact]
        public async Task GetCustomerInvoicesAsync_Should_Return_Empty_When_No_Invoice_Matches_Current_Unit()
        {
            const int customerId = 77774;

            await ClearInvoiceTablesAsync();
            await SeedInvoiceWithBagWeightAsync(customerId, bagWeight: 1m, netWeight: 1m, unitId: 50);

            var rows = await CreateRepo(
                    uomLookup: BuildEmptyUomLookup(),
                    ipService: BuildIpServiceWithUnit(1))
                .GetCustomerInvoicesAsync(customerId);

            rows.Should().BeEmpty();
        }
    }
}
