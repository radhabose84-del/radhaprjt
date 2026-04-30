using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesOrder;
using SalesManagement.IntegrationTests.Common;
using System.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesOrder
{
    /// <summary>
    /// Integration tests for SalesOrderCommandRepository.
    /// Verifies EF Core Create, Update, Cancel, Foreclose, and attachment operations
    /// against a real SQL Server database.
    ///
    /// SalesOrderCommandRepository requires:
    ///   - IMiscMasterQueryRepository (for status lookups: Pending, Open, Cancelled, ForeClosed)
    ///   - IDocumentSequenceLookup (for IncrementDocNoAsync within transaction)
    ///   - IIPAddressService (for Cancel/Foreclose audit fields)
    ///
    /// Prerequisites: SalesGroup chain (SalesOrganisation → SalesOffice → SalesGroup),
    ///   MiscTypeMaster + MiscMaster rows for ApprovalStatus/LineItemStatus types.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesOrderCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesOrderCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ── Factory ───────────────────────────────────────────────────────

        private SalesOrderCommandRepository CreateRepo(
            ApplicationDbContext ctx,
            Mock<IMiscMasterQueryRepository>? miscRepo = null,
            Mock<IDocumentSequenceLookup>? docSeq = null,
            Mock<IIPAddressService>? ipSvc = null)
        {
            miscRepo ??= BuildDefaultMiscRepoMock();

            if (docSeq == null)
            {
                docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
                docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                    .Returns(Task.CompletedTask);
            }

            if (ipSvc == null)
            {
                ipSvc = new Mock<IIPAddressService>(MockBehavior.Loose);
                ipSvc.Setup(x => x.GetUserName()).Returns("test-user");
                ipSvc.Setup(x => x.GetUserIPAddress()).Returns("127.0.0.1");
            }

            return new SalesOrderCommandRepository(ctx, miscRepo.Object, docSeq.Object, ipSvc.Object);
        }

        private Mock<IMiscMasterQueryRepository> BuildDefaultMiscRepoMock()
        {
            var mock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);

            // Return real DB IDs seeded by EnsurePrerequisitesAsync
            mock.Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string typeCode, string code) =>
                    new SalesManagement.Domain.Entities.MiscMaster
                    {
                        Id = code switch
                        {
                            "Pending" => _pendingStatusId,
                            "Open" => _openStatusId,
                            "Cancelled" => _cancelledStatusId,
                            "ForeClosed" => _foreclosedStatusId,
                            _ => _pendingStatusId
                        },
                        Code = code,
                        Description = code,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    });

            return mock;
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

        // Real DB IDs for status MiscMaster rows - populated by EnsurePrerequisitesAsync
        private int _pendingStatusId, _openStatusId, _cancelledStatusId, _foreclosedStatusId;

        private async Task<(int salesGroupId, int enquiryTypeId, int freightTypeId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // MiscTypeMaster for misc references
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "SOC_MT");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "SOC_MT", Description = "SO Cmd Misc",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }

            var enquiryTypeId = await EnsureMiscAsync(ctx, mt.Id, "SOC_ENQ");
            var freightTypeId = await EnsureMiscAsync(ctx, mt.Id, "SOC_FRT");

            // Seed status rows that the repository uses via IMiscMasterQueryRepository
            _pendingStatusId = await EnsureMiscAsync(ctx, mt.Id, "Pending");
            _openStatusId = await EnsureMiscAsync(ctx, mt.Id, "Open");
            _cancelledStatusId = await EnsureMiscAsync(ctx, mt.Id, "Cancelled");
            _foreclosedStatusId = await EnsureMiscAsync(ctx, mt.Id, "ForeClosed");

            // SalesOrganisation → SalesOffice → SalesGroup chain
            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "SOCORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "SOCORG", SalesOrganisationName = "SOC Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }

            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "SOC_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "SOC_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }

            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "SOC_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "SOC_SG", SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }

            return (sg.Id, enquiryTypeId, freightTypeId);
        }

        private SalesManagement.Domain.Entities.SalesOrderHeader BuildEntity(
            int salesGroupId, int enquiryTypeId, int freightTypeId,
            string salesOrderNo = "SOC_01",
            int partyId = 100,
            int detailCount = 2)
        {
            return new SalesManagement.Domain.Entities.SalesOrderHeader
            {
                SalesOrderNo = salesOrderNo,
                OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                SalesGroupId = salesGroupId,
                EnquiryType = enquiryTypeId,
                UnitId = 1,
                PartyId = partyId,
                FreightTypeId = freightTypeId,
                FinalAmount = 5000m,
                TotalBags = 10,
                TotalWeightKgs = 500m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                SalesOrderDetails = Enumerable.Range(1, detailCount).Select(i =>
                    new SalesManagement.Domain.Entities.SalesOrderDetail
                    {
                        ItemId = i * 10,
                        HSNId = 1,
                        QtyInBags = 5,
                        BagWeight = 50m,
                        SaleUOMId = 1,
                        TotalWeight = 250m,
                        ExMillRate = 100m,
                        TaxableAmount = 250m,
                        TaxPercentage = 5m,
                        TaxAmount = 12.5m,
                        NetAmount = 262.5m,
                        ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(10))
                    }).ToList()
            };
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync(
                "Sales.SalesOrderDiscount",
                "Sales.SalesOrderDetail",
                "Sales.SalesOrderHeader");

        // ── CreateAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_C1");
            var newId = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Details()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_C2", detailCount: 3);
            var newId = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.Id == newId);
            saved.Should().NotBeNull();
            saved!.SalesOrderNo.Should().Be("SOC_C2");
            saved.PartyId.Should().Be(100);

            var details = await ctx.SalesOrderDetail.Where(d => d.SalesOrderHeaderId == newId).ToListAsync();
            details.Should().HaveCount(3);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_StatusId_To_Pending()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_C3");
            var newId = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.Id == newId);
            // StatusId is set by the mock returning 9001 for "Pending"
            saved!.StatusId.Should().Be(_pendingStatusId);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_C4");
            var newId = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.Id == newId);
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Persist_Header_Changes()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_U1");
            var id = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_U1", partyId: 200, detailCount: 1);
            updated.Id = id;
            updated.Remarks = "updated remarks";
            updated.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.Id == id);
            reloaded!.PartyId.Should().Be(200);
            reloaded.Remarks.Should().Be("updated remarks");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Details()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_U2", detailCount: 2);
            var id = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_U2", detailCount: 4);
            updated.Id = id;

            await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var details = await ctx.SalesOrderDetail.Where(d => d.SalesOrderHeaderId == id).ToListAsync();
            details.Should().HaveCount(4);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = BuildEntity(1, 1, 1, salesOrderNo: "SOC_GHOST");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        // ── CancelAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task CancelAsync_Should_Return_True_When_Successful()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_CN1");
            var id = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).CancelAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CancelAsync_Should_Set_CancelledFields()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_CN2");
            var id = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).CancelAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.Id == id);
            saved!.CancelledDate.Should().NotBeNull();
            saved.CancelledByName.Should().Be("test-user");
            saved.CancelledIP.Should().Be("127.0.0.1");
            saved.StatusId.Should().Be(_cancelledStatusId);
        }

        [Fact]
        public async Task CancelAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).CancelAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // ── ForecloseAsync ────────────────────────────────────────────────

        [Fact]
        public async Task ForecloseAsync_Should_Return_True_When_Successful()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_FC1");
            var id = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).ForecloseAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ForecloseAsync_Should_Set_ForeClosedFields()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_FC2");
            var id = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).ForecloseAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.Id == id);
            saved!.ForeClosedDate.Should().NotBeNull();
            saved.ForeClosedByName.Should().Be("test-user");
            saved.ForeClosedIP.Should().Be("127.0.0.1");
            saved.StatusId.Should().Be(_foreclosedStatusId);
        }

        [Fact]
        public async Task ForecloseAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).ForecloseAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // ── UpdateVisitNotesAttachmentAsync ────────────────────────────────

        [Fact]
        public async Task UpdateVisitNotesAttachmentAsync_Should_Persist_FileName()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_VN1");
            var id = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).UpdateVisitNotesAttachmentAsync(id, "notes.pdf", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var saved = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.Id == id);
            saved!.VisitNotesAttachment.Should().Be("notes.pdf");
        }

        [Fact]
        public async Task UpdateVisitNotesAttachmentAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateVisitNotesAttachmentAsync(9999999, "f.pdf", CancellationToken.None);

            result.Should().BeFalse();
        }

        // ── UpdateAgentPOAttachmentAsync ───────────────────────────────────

        [Fact]
        public async Task UpdateAgentPOAttachmentAsync_Should_Persist_FileName()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_AP1");
            var id = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).UpdateAgentPOAttachmentAsync(id, "po.pdf", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var saved = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.Id == id);
            saved!.AgentPOAttachment.Should().Be("po.pdf");
        }

        [Fact]
        public async Task UpdateAgentPOAttachmentAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateAgentPOAttachmentAsync(9999999, "f.pdf", CancellationToken.None);

            result.Should().BeFalse();
        }

        // ── GetByIdEntityAsync ────────────────────────────────────────────

        [Fact]
        public async Task GetByIdEntityAsync_Should_Return_Entity()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_GBE1");
            var id = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).GetByIdEntityAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.SalesOrderNo.Should().Be("SOC_GBE1");
        }

        [Fact]
        public async Task GetByIdEntityAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetByIdEntityAsync(9999999);

            result.Should().BeNull();
        }

        // ── FinalizeOrderStatusAsync ──────────────────────────────────────

        [Fact]
        public async Task FinalizeOrderStatusAsync_Should_Update_StatusId()
        {
            await ClearAsync();
            var (sgId, enqId, frtId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity(sgId, enqId, frtId, salesOrderNo: "SOC_FOS1");
            var id = await CreateRepo(ctx).CreateAsync(entity, transactionTypeId: 1);
            ctx.ChangeTracker.Clear();

            var forStatus = new SalesManagement.Domain.Entities.SalesOrderHeader
            {
                Id = id,
                StatusId = _openStatusId
            };

            var result = await CreateRepo(ctx).FinalizeOrderStatusAsync(forStatus, 7, "tester", "127.0.0.1");
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var saved = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.Id == id);
            saved!.StatusId.Should().Be(_openStatusId);
        }

        [Fact]
        public async Task FinalizeOrderStatusAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = new SalesManagement.Domain.Entities.SalesOrderHeader { Id = 9999999, StatusId = 1 };
            var result = await CreateRepo(ctx).FinalizeOrderStatusAsync(ghost, 7, "tester", "127.0.0.1");

            result.Should().BeFalse();
        }
    }
}
