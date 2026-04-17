using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Updates.Party;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DispatchAdvice;
using SalesManagement.IntegrationTests.Common;
using System.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DispatchAdvice
{
    /// <summary>
    /// Integration tests for DispatchAdviceCommandRepository.
    ///
    /// DispatchAdviceCommandRepository.CreateAsync is a complex transactional operation that:
    ///   1. Inserts header + details
    ///   2. Updates StockLedger rows (Packed → Reserved) per detail pack range
    ///   3. Propagates freight to PartyMaster or DispatchAddressMaster
    ///   4. Increments Finance.DocumentSequence
    ///
    /// Full Create/SoftDelete testing requires a populated StockLedger with PROD-type records
    /// matching ItemId, LotId, PackNo, PackTypeId, and StatusId. This fixture-level setup is
    /// beyond the scope of repository-level integration tests (covered by end-to-end workflow tests).
    ///
    /// These tests validate the simpler repository paths: header persistence, soft delete on
    /// records without StockLedger dependencies, and not-found edge cases.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class DispatchAdviceCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DispatchAdviceCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DispatchAdviceCommandRepository CreateRepo(
            ApplicationDbContext ctx,
            Mock<IDocumentSequenceLookup>? docSeq = null,
            Mock<IPartyFreightUpdate>? partyFreight = null)
        {
            if (docSeq == null)
            {
                docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
                docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                    .Returns(Task.CompletedTask);
            }

            partyFreight ??= new Mock<IPartyFreightUpdate>(MockBehavior.Loose);

            return new DispatchAdviceCommandRepository(ctx, docSeq.Object, partyFreight.Object);
        }

        // ── Prerequisites ─────────────────────────────────────────────────

        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx, int miscTypeId, string code)
        {
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<(int salesOrderId, int statusId, int dispatchTypeId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // MiscType for dispatch-related statuses
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "DAC_MT");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "DAC_MT", Description = "DA Cmd Misc",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }

            var statusId = await EnsureMiscAsync(ctx, mt.Id, "DAC_ST");
            var dispatchTypeId = await EnsureMiscAsync(ctx, mt.Id, "DAC_DT");

            // SalesOrganisation → SalesOffice → SalesGroup for SalesOrder FK
            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "DACORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "DACORG", SalesOrganisationName = "DA Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }

            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "DAC_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "DAC_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }

            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "DAC_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "DAC_SG", SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }

            // SalesOrder header (parent FK for DispatchAdvice)
            var so = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.SalesOrderNo == "DAC_SO1");
            if (so == null)
            {
                so = new SalesManagement.Domain.Entities.SalesOrderHeader
                {
                    SalesOrderNo = "DAC_SO1",
                    OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    SalesGroupId = sg.Id,
                    EnquiryType = statusId,
                    UnitId = 1,
                    PartyId = 100,
                    FreightTypeId = statusId,
                    FinalAmount = 5000m,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrderHeader.AddAsync(so);
                await ctx.SaveChangesAsync();
            }

            return (so.Id, statusId, dispatchTypeId);
        }

        private SalesManagement.Domain.Entities.DispatchAdviceHeader BuildEntity(
            int salesOrderId, int statusId, int dispatchTypeId,
            string dispatchNo = "DAC_01")
        {
            return new SalesManagement.Domain.Entities.DispatchAdviceHeader
            {
                DispatchNo = dispatchNo,
                DispatchDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                StatusId = statusId,
                SalesOrderId = salesOrderId,
                PartyId = 100,
                TotOrderQty = 100m,
                TotDispatchedQty = 50m,
                TotPendingQty = 50m,
                DispatchTypeId = dispatchTypeId,
                FreightId = 1,
                UnitId = 1,
                VehicleNo = "TN01AB1234",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                // No details — avoids StockLedger dependency
                DispatchAdviceDetails = new List<SalesManagement.Domain.Entities.DispatchAdviceDetail>()
            };
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync(
                "Sales.DispatchAdviceDetail",
                "Sales.DispatchAdviceHeader");

        // ── CreateAsync (header-only, no StockLedger) ─────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearAsync();
            var (soId, statusId, dtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(soId, statusId, dtId, dispatchNo: "DAC_C1");
            var newId = await CreateRepo(ctx).CreateAsync(
                entity, unitId: 1, packedStatusId: 0, reservedStatusId: 0,
                transactionTypeId: 1, dispatchTypeName: null,
                directToPartyName: "DirectToParty", othersName: "Others");

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_Fields()
        {
            await ClearAsync();
            var (soId, statusId, dtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(soId, statusId, dtId, dispatchNo: "DAC_C2");
            var newId = await CreateRepo(ctx).CreateAsync(
                entity, unitId: 1, packedStatusId: 0, reservedStatusId: 0,
                transactionTypeId: 1, dispatchTypeName: null,
                directToPartyName: "DirectToParty", othersName: "Others");
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DispatchAdviceHeader.FirstOrDefaultAsync(x => x.Id == newId);
            saved.Should().NotBeNull();
            saved!.DispatchNo.Should().Be("DAC_C2");
            saved.SalesOrderId.Should().Be(soId);
            saved.PartyId.Should().Be(100);
            saved.VehicleNo.Should().Be("TN01AB1234");
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAsync();
            var (soId, statusId, dtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(soId, statusId, dtId, dispatchNo: "DAC_C3");
            var newId = await CreateRepo(ctx).CreateAsync(
                entity, unitId: 1, packedStatusId: 0, reservedStatusId: 0,
                transactionTypeId: 1, dispatchTypeName: null,
                directToPartyName: "DirectToParty", othersName: "Others");
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DispatchAdviceHeader.FirstOrDefaultAsync(x => x.Id == newId);
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── SoftDeleteAsync ───────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(
                9999999, reservedStatusId: 0, packedStatusId: 0, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_On_HeaderOnly_Record()
        {
            await ClearAsync();
            var (soId, statusId, dtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            // Create header without detail lines (no StockLedger interaction)
            var entity = BuildEntity(soId, statusId, dtId, dispatchNo: "DAC_SD1");
            var newId = await CreateRepo(ctx).CreateAsync(
                entity, unitId: 1, packedStatusId: 0, reservedStatusId: 0,
                transactionTypeId: 1, dispatchTypeName: null,
                directToPartyName: "DirectToParty", othersName: "Others");
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(
                newId, reservedStatusId: 0, packedStatusId: 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var saved = await ctx.DispatchAdviceHeader
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);
            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
