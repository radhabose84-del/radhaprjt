using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.Invoice;
using SalesManagement.IntegrationTests.Common;
using System.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.Invoice
{
    /// <summary>
    /// Integration tests for InvoiceCommandRepository.
    ///
    /// InvoiceCommandRepository.CreateAsync is a complex transactional operation that:
    ///   1. Inserts header + details
    ///   2. Updates StockLedger rows (Dispatched → Invoiced) via DispatchAdviceDetail pack ranges
    ///   3. Sets DispatchAdviceHeader.InvFlg = true
    ///   4. Increments Finance.DocumentSequence
    ///
    /// Full Create/Update testing requires populated StockLedger + DispatchAdvice chain.
    /// These tests validate the simpler operations: UpdateApprovalStatusAsync,
    /// UpdateInvoiceStatusIdAsync, and not-found edge cases against a minimal fixture.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class InvoiceCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public InvoiceCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private InvoiceCommandRepository CreateRepo(
            ApplicationDbContext ctx,
            Mock<IDocumentSequenceLookup>? docSeq = null)
        {
            if (docSeq == null)
            {
                docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
                docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                    .Returns(Task.CompletedTask);
            }
            return new InvoiceCommandRepository(ctx, docSeq.Object);
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

        /// <summary>
        /// Seeds MiscTypeMaster("ApprovalStatus") + MiscMaster("Pending","Approved") +
        /// a minimal DispatchAdvice chain (SalesOrder → DispatchAdvice) for invoice FK references,
        /// then returns (dispatchAdviceId, approvalStatusTypeId, pendingStatusId, approvedStatusId).
        /// </summary>
        private async Task<(int daId, int statusMiscTypeId, int pendingId, int approvedId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // ApprovalStatus MiscType
            var approvalType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "ApprovalStatus");
            if (approvalType == null)
            {
                approvalType = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ApprovalStatus", Description = "Approval Status",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(approvalType);
                await ctx.SaveChangesAsync();
            }
            var pendingId = await EnsureMiscAsync(ctx, approvalType.Id, "Pending");
            var approvedId = await EnsureMiscAsync(ctx, approvalType.Id, "Approved");

            // Auxiliary MiscType for SalesOrder FKs
            var auxType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "IVC_AUX");
            if (auxType == null)
            {
                auxType = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "IVC_AUX", Description = "Invoice Aux",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(auxType);
                await ctx.SaveChangesAsync();
            }
            var auxMiscId = await EnsureMiscAsync(ctx, auxType.Id, "IVC_M1");

            // SalesGroup chain
            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "IVCORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "IVCORG", SalesOrganisationName = "IVC Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "IVC_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "IVC_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "IVC_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "IVC_SG", SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }

            // SalesOrder
            var so = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.SalesOrderNo == "IVC_SO1");
            if (so == null)
            {
                so = new SalesManagement.Domain.Entities.SalesOrderHeader
                {
                    SalesOrderNo = "IVC_SO1",
                    OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    SalesGroupId = sg.Id, EnquiryType = auxMiscId,
                    UnitId = 1, PartyId = 100, FreightTypeId = auxMiscId,
                    FinalAmount = 5000m,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrderHeader.AddAsync(so);
                await ctx.SaveChangesAsync();
            }

            // DispatchAdvice (parent FK for Invoice)
            var da = await ctx.DispatchAdviceHeader.FirstOrDefaultAsync(x => x.DispatchNo == "IVC_DA1");
            if (da == null)
            {
                da = new SalesManagement.Domain.Entities.DispatchAdviceHeader
                {
                    DispatchNo = "IVC_DA1",
                    DispatchDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    StatusId = pendingId, SalesOrderId = so.Id,
                    PartyId = 100, TotOrderQty = 100m, TotDispatchedQty = 50m,
                    TotPendingQty = 50m, DispatchTypeId = auxMiscId,
                    FreightId = 1, UnitId = 1,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.DispatchAdviceHeader.AddAsync(da);
                await ctx.SaveChangesAsync();
            }

            return (da.Id, approvalType.Id, pendingId, approvedId);
        }

        private async Task<int> SeedInvoiceAsync(int dispatchAdviceId, int? statusId = null, string invoiceNo = "IVC_01")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var invoice = new SalesManagement.Domain.Entities.InvoiceHeader
            {
                InvoiceNo = invoiceNo,
                InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                DispatchAdviceId = dispatchAdviceId,
                PartyId = 100, UnitId = 1, FinancialYearId = 1,
                TotalBags = 10, TotalWeight = 500m,
                TaxableValue = 5000m, InvoiceAmount = 5250m,
                StatusId = statusId,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.InvoiceHeader.AddAsync(invoice);
            await ctx.SaveChangesAsync();
            return invoice.Id;
        }

        private async Task ClearInvoicesAsync() =>
            await _fixture.ClearTablesAsync(
                "Sales.InvoiceDetail",
                "Sales.InvoiceHeader");

        // ── UpdateApprovalStatusAsync ─────────────────────────────────────

        [Fact]
        public async Task UpdateApprovalStatusAsync_Should_Update_StatusId()
        {
            await ClearInvoicesAsync();
            var (daId, _, pendingId, _) = await EnsurePrerequisitesAsync();
            var invoiceId = await SeedInvoiceAsync(daId, statusId: pendingId, invoiceNo: "IVC_UAS1");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).UpdateApprovalStatusAsync(invoiceId, "Approved", 7, "tester", "127.0.0.1", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.InvoiceHeader.FirstOrDefaultAsync(x => x.Id == invoiceId);
            saved.Should().NotBeNull();
            // StatusId should now point to the "Approved" MiscMaster row
            saved!.StatusId.Should().NotBe(pendingId);
        }

        [Fact]
        public async Task UpdateApprovalStatusAsync_Should_NoOp_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // Should not throw
            await CreateRepo(ctx).UpdateApprovalStatusAsync(9999999, "Approved", 7, "tester", "127.0.0.1", CancellationToken.None);
        }

        // ── UpdateInvoiceStatusIdAsync ────────────────────────────────────

        [Fact]
        public async Task UpdateInvoiceStatusIdAsync_Should_Set_StatusId()
        {
            await ClearInvoicesAsync();
            var (daId, _, pendingId, approvedId) = await EnsurePrerequisitesAsync();
            var invoiceId = await SeedInvoiceAsync(daId, statusId: pendingId, invoiceNo: "IVC_USI1");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).UpdateInvoiceStatusIdAsync(invoiceId, approvedId, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.InvoiceHeader.FirstOrDefaultAsync(x => x.Id == invoiceId);
            saved!.StatusId.Should().Be(approvedId);
        }

        [Fact]
        public async Task UpdateInvoiceStatusIdAsync_Should_NoOp_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // Should not throw
            await CreateRepo(ctx).UpdateInvoiceStatusIdAsync(9999999, 1, CancellationToken.None);
        }

        // ── GetByIdInvoiceWorkFlowAsync ───────────────────────────────────

        [Fact]
        public async Task GetByIdInvoiceWorkFlowAsync_Should_Return_Dto()
        {
            await ClearInvoicesAsync();
            var (daId, _, pendingId, _) = await EnsurePrerequisitesAsync();
            var invoiceId = await SeedInvoiceAsync(daId, statusId: pendingId, invoiceNo: "IVC_WF1");

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetByIdInvoiceWorkFlowAsync(invoiceId);

            result.Should().NotBeNull();
            result.Id.Should().Be(invoiceId);
            result.InvoiceNo.Should().Be("IVC_WF1");
        }
    }
}
