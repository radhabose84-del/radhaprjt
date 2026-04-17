using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PurchaseManagement.Application.Common.Interfaces.ILogService;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.PurchaseIndents;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseIndents
{
    [Collection("DatabaseCollection")]
    public sealed class PurchaseIndentCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public PurchaseIndentCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PurchaseIndentCommandRepository CreateRepo(
            ApplicationDbContext ctx,
            Mock<ILogServiceCommand>? logSvc = null,
            Mock<IMiscMasterQueryRepository>? miscRepo = null,
            Mock<IMapper>? mapper = null)
        {
            logSvc ??= new Mock<ILogServiceCommand>(MockBehavior.Loose);
            miscRepo ??= new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            mapper ??= new Mock<IMapper>(MockBehavior.Loose);

            // Default: misc lookups return a stub MiscMaster so RollbackStatus / Update can run
            miscRepo.Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 0 });

            var logger = NullLogger<PurchaseIndentCommandRepository>.Instance;
            return new PurchaseIndentCommandRepository(ctx, logSvc.Object, miscRepo.Object, mapper.Object, logger);
        }

        private async Task<int> EnsureMiscIdAsync(string code, string typeCode = "PI_TT")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == typeCode);
            if (t == null)
            {
                t = new PurchaseManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = typeCode,
                    Description = typeCode,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == code && x.MiscTypeId == t.Id);
            if (m == null)
            {
                m = new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id,
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

        private async Task<IndentHeader> BuildHeaderAsync(string indentNumber)
        {
            var typeId = await EnsureMiscIdAsync("PI_NORMAL", "PI_TT");
            var statusId = await EnsureMiscIdAsync("PI_OPEN", "PI_ST");
            return new IndentHeader
            {
                IndentNumber = indentNumber,
                IndentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IndentTypeId = typeId,
                UnitId = 1,
                DepartmentId = 1,
                Purpose = "Test purpose",
                StatusId = statusId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                IndentDetails = new List<IndentDetail>
                {
                    new()
                    {
                        ItemId = 1,
                        ItemCategoryId = 1,
                        ItemUOMId = 1,
                        Rate = 10m,
                        QuantityRequired = 5m,
                        RequiredDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                        TotalEstimatedCost = 50m,
                        PRConsumptionDays = 7,
                        Remark = "test",
                        IsRFQDone = false,
                        StatusId = statusId,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }
                }
            };
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var result = await CreateRepo(ctx).CreateAsync(await BuildHeaderAsync("PI_C1"));

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var result = await CreateRepo(ctx).CreateAsync(await BuildHeaderAsync("PI_C2"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.IndentHeader.Include(h => h.IndentDetails).FirstAsync(h => h.Id == result.Id);

            saved.IndentNumber.Should().Be("PI_C2");
            saved.IndentDetails.Should().ContainSingle();
            saved.IndentDetails!.First().QuantityRequired.Should().Be(5m);
        }

        // --- DeleteAsync (soft) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var created = await CreateRepo(ctx).CreateAsync(await BuildHeaderAsync("PI_D1"));
            ctx.ChangeTracker.Clear();

            var entity = await BuildHeaderAsync("PI_D1");
            entity.IsDeleted = IsDelete.Deleted;

            var result = await CreateRepo(ctx).DeleteAsync(created.Id, entity);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Flag_IsDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var created = await CreateRepo(ctx).CreateAsync(await BuildHeaderAsync("PI_D2"));
            ctx.ChangeTracker.Clear();

            var entity = await BuildHeaderAsync("PI_D2");
            entity.IsDeleted = IsDelete.Deleted;
            await CreateRepo(ctx).DeleteAsync(created.Id, entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.IndentHeader.FirstAsync(x => x.Id == created.Id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildHeaderAsync("GH");
            entity.IsDeleted = IsDelete.Deleted;

            var result = await CreateRepo(ctx).DeleteAsync(9999999, entity);

            result.Should().BeFalse();
        }

        // --- FinalizeStatus ---

        [Fact]
        public async Task FinalizeStatus_Should_Return_True_And_Apply_Status_To_All_Lines_When_No_Detail_List()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var created = await CreateRepo(ctx).CreateAsync(await BuildHeaderAsync("PI_F1"));
            var newStatusId = await EnsureMiscIdAsync("PI_APPROVED", "PI_ST");
            ctx.ChangeTracker.Clear();

            var update = new IndentHeader
            {
                Id = created.Id,
                StatusId = newStatusId,
                IndentNumber = "PI_F1",
                Purpose = "p",
                IndentDetails = new List<IndentDetail>()
            };
            var result = await CreateRepo(ctx).FinalizeStatus(update);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.IndentHeader.Include(h => h.IndentDetails).FirstAsync(x => x.Id == created.Id);
            reloaded.StatusId.Should().Be(newStatusId);
            reloaded.IndentDetails.Should().AllSatisfy(d => d.StatusId.Should().Be(newStatusId));
        }

        [Fact]
        public async Task FinalizeStatus_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).FinalizeStatus(new IndentHeader
            {
                Id = 9999999,
                StatusId = 1,
                IndentNumber = "GH",
                Purpose = "p",
                IndentDetails = new List<IndentDetail>()
            });

            result.Should().BeFalse();
        }

        // --- UpdateRFQStatusAsync ---

        [Fact]
        public async Task UpdateRFQStatusAsync_Should_Mark_All_Specified_Details_RFQDone()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var created = await CreateRepo(ctx).CreateAsync(await BuildHeaderAsync("PI_RFQ1"));
            ctx.ChangeTracker.Clear();

            var detailId = (await ctx.IndentDetail.Where(d => d.IndentHeaderId == created.Id).FirstAsync()).Id;
            var result = await CreateRepo(ctx).UpdateRFQStatusAsync(new[] { detailId });
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.IndentDetail.FirstAsync(d => d.Id == detailId);
            reloaded.IsRFQDone.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateRFQStatusAsync_Should_Return_False_When_Empty_Input()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateRFQStatusAsync(Array.Empty<int>());

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateRFQStatusAsync_Should_Return_False_When_Ids_Dont_Match()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var result = await CreateRepo(ctx).UpdateRFQStatusAsync(new[] { 9999999 });

            result.Should().BeFalse();
        }
    }
}
