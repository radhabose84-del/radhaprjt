using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.ProformaInvoice;
using SalesManagement.IntegrationTests.Common;
using System.Data;
using System.Data.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.ProformaInvoice
{
    [Collection("DatabaseCollection")]
    public sealed class ProformaInvoiceCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public ProformaInvoiceCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ProformaInvoiceCommandRepository CreateRepo(ApplicationDbContext ctx, Mock<IDocumentSequenceLookup>? docSeq = null)
        {
            if (docSeq == null)
            {
                docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
                docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                    .Returns(Task.CompletedTask);
            }
            return new ProformaInvoiceCommandRepository(ctx, docSeq.Object);
        }

        private async Task<(int soId, int partyId)> EnsureSalesOrderAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "PIC_ORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "PIC_ORG", SalesOrganisationName = "Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "PIC_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "PIC_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "PIC_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "PIC_SG", SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }

            // Misc types used by SalesOrderHeader FKs
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "PIC_MT");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "PIC_MT", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            var freight = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "PIC_FT");
            if (freight == null)
            {
                freight = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = mt.Id, Code = "PIC_FT", Description = "Freight",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(freight);
                await ctx.SaveChangesAsync();
            }
            var enq = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "PIC_ENQ");
            if (enq == null)
            {
                enq = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = mt.Id, Code = "PIC_ENQ", Description = "Unit",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(enq);
                await ctx.SaveChangesAsync();
            }

            var so = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.SalesOrderNo == "PIC_SO1");
            if (so == null)
            {
                so = new SalesManagement.Domain.Entities.SalesOrderHeader
                {
                    SalesOrderNo = "PIC_SO1",
                    OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    SalesGroupId = sg.Id,
                    EnquiryType = enq.Id,
                    UnitId = 1,
                    PartyId = 100,
                    FreightTypeId = freight.Id,
                    FinalAmount = 1000m,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrderHeader.AddAsync(so);
                await ctx.SaveChangesAsync();
            }
            return (so.Id, 100);
        }

        private async Task<SalesManagement.Domain.Entities.ProformaInvoice> BuildEntityAsync(
            string number = "PI001", decimal amount = 500m)
        {
            var (soId, partyId) = await EnsureSalesOrderAsync();
            return new SalesManagement.Domain.Entities.ProformaInvoice
            {
                ProformaNumber = number,
                ProformaDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                SalesOrderId = soId,
                PartyId = partyId,
                ProformaAmount = amount,
                SOBalance = 500m,
                PaymentReceivedAmount = 0m,
                Remarks = "test",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("PI_C1"), transactionTypeId: 1);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Call_IncrementDocNo()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                .Returns(Task.CompletedTask);

            await CreateRepo(ctx, docSeq).CreateAsync(await BuildEntityAsync("PI_C2"), transactionTypeId: 7);

            docSeq.Verify(d => d.IncrementDocNoAsync(7, It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("PI_U1"), 1);
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync("PI_U1");
            entity.Id = id;
            entity.Remarks = "updated remarks";
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.ProformaInvoice.FirstAsync(x => x.Id == id);
            reloaded.Remarks.Should().Be("updated remarks");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_Immutable_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("PI_IMM", amount: 500m), 1);
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync("DIFFERENT", amount: 999m);
            entity.Id = id;

            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.ProformaInvoice.FirstAsync(x => x.Id == id);
            reloaded.ProformaNumber.Should().Be("PI_IMM"); // immutable
            reloaded.ProformaAmount.Should().Be(500m);      // immutable
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = await BuildEntityAsync("GHOST");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdatePaymentAsync_Should_Persist_Payment_And_Status()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("PI_P1"), 1);
            ctx.ChangeTracker.Clear();

            // Use existing MiscMaster seeded by EnsureSalesOrderAsync (PIC_FT) as status
            var validStatusId = await ctx.MiscMaster.Where(x => x.Code == "PIC_FT").Select(x => x.Id).FirstAsync();

            var result = await CreateRepo(ctx).UpdatePaymentAsync(id, 250m, statusId: validStatusId);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.ProformaInvoice.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            reloaded.PaymentReceivedAmount.Should().Be(250m);
            reloaded.StatusId.Should().Be(validStatusId);
        }

        [Fact]
        public async Task UpdatePaymentAsync_Should_Skip_Status_When_Null()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("PI_P2"), 1);
            ctx.ChangeTracker.Clear();

            var before = await ctx.ProformaInvoice.FirstAsync(x => x.Id == id);
            var originalStatus = before.StatusId;
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).UpdatePaymentAsync(id, 100m, statusId: null);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.ProformaInvoice.FirstAsync(x => x.Id == id);
            reloaded.StatusId.Should().Be(originalStatus);
            reloaded.PaymentReceivedAmount.Should().Be(100m);
        }

        [Fact]
        public async Task UpdatePaymentAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdatePaymentAsync(9999999, 100m, 1);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("PI_D1"), 1);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.ProformaInvoice.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
