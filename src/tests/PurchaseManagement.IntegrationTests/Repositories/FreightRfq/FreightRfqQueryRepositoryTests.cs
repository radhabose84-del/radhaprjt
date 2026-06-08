using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.FreightRfq;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.FreightRfq;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.FreightRfq
{
    [Collection("DatabaseCollection")]
    public sealed class FreightRfqQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IFinancialYearLookup> _fyMock = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _miscMock = new(MockBehavior.Loose);
        private readonly Mock<ISupplierLookup> _supplierMock = new(MockBehavior.Loose);
        private readonly Mock<ITransporterLookup> _transporterMock = new(MockBehavior.Loose);

        private static readonly DateTimeOffset RfqDate = new(2025, 6, 6, 0, 0, 0, TimeSpan.Zero);

        public FreightRfqQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
            _fyMock.Setup(f => f.GetAllFinancialYearAsync()).ReturnsAsync(new List<FinancialYearLookupDto>
            {
                new() { FinancialYearId = 1, StartYear = "2025", StartDate = new DateTime(2025, 4, 1), EndDate = new DateTime(2026, 3, 31), IsActive = true }
            });
            _transporterMock.Setup(t => t.SearchTransportersAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransporterLookupDto>
                {
                    new() { Id = 101, TransporterCode = "T101", TransporterName = "Sri Venkateswara Roadways" },
                    new() { Id = 102, TransporterCode = "T102", TransporterName = "Blue Dart Surface Logistics" }
                });
        }

        private FreightRfqQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString), _fyMock.Object, _supplierMock.Object, _transporterMock.Object);

        private async Task<(int PoBasedId, int DraftId, int PerMtId)> SeedMiscAsync(ApplicationDbContext ctx)
        {
            await _fixture.ClearAllTablesAsync();
            var typeRepo = new MiscTypeMasterCommandRepository(ctx);
            var miscRepo = new MiscMasterCommandRepository(ctx);

            async Task<int> Misc(int typeId, string code) =>
                (await miscRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscMaster
                { MiscTypeId = typeId, Code = code, Description = code, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted })).Id;

            var typeType = (await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            { MiscTypeCode = MiscEnumEntity.FreightRfqType, Description = "Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted })).Id;
            var statusType = (await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            { MiscTypeCode = MiscEnumEntity.FreightRfqStatus, Description = "Status", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted })).Id;
            var basisType = (await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            { MiscTypeCode = MiscEnumEntity.FreightRateBasis, Description = "Basis", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted })).Id;

            var poBased = await Misc(typeType, MiscEnumEntity.FreightRfqTypePoBased);
            var draft = await Misc(statusType, MiscEnumEntity.Draft);
            var perMt = await Misc(basisType, MiscEnumEntity.FreightRateBasisPerMt);

            async Task<PurchaseManagement.Domain.Entities.MiscMaster> Load(int id) =>
                await ctx.MiscMaster.AsNoTracking().FirstAsync(m => m.Id == id);
            _miscMock.Setup(m => m.GetMiscMasterByName(MiscEnumEntity.FreightRfqStatus, MiscEnumEntity.Draft)).ReturnsAsync(await Load(draft));

            return (poBased, draft, perMt);
        }

        private async Task<int> SeedRfqWithQuotesAsync(ApplicationDbContext ctx, int poBasedId, int perMtId)
        {
            var repo = new FreightRfqCommandRepository(ctx, _fyMock.Object, _miscMock.Object);
            var id = await repo.CreateAsync(new FreightRfqHeader
            {
                RfqDate = RfqDate,
                RfqTypeId = poBasedId,
                SupplierId = 5,
                SourceLocation = "Adilabad",
                SourceStation = "Adilabad Yard",
                DestinationLocation = "Dindigul",
                DestinationStation = "Dindigul Mill Gate",
                TotalQuantity = 120.5m,
                TotalBaleCount = 700,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            await repo.SaveQuotationsAsync(id, new List<FreightRfqQuotation>
            {
                new() { TransporterId = 101, RateBasisId = perMtId, QuotedRate = 5000m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted },
                new() { TransporterId = 102, RateBasisId = perMtId, QuotedRate = 2000m, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted }
            });
            return id;
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Header_With_Quotations_And_Derived()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (poBased, _, perMt) = await SeedMiscAsync(ctx);
            var id = await SeedRfqWithQuotesAsync(ctx, poBased, perMt);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.FreightRfqNumber.Should().Be("FRFQ-2025-0001");
            dto.StatusName.Should().Be("Draft");
            dto.Quotations.Should().HaveCount(2);
            dto.LowestQuotedRate.Should().Be(2000m);
            dto.HighestQuotedRate.Should().Be(5000m);
            dto.Quotations.Should().Contain(q => q.TransporterName == "Blue Dart Surface Logistics");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Row_With_Route_And_QuotesCount()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (poBased, _, perMt) = await SeedMiscAsync(ctx);
            await SeedRfqWithQuotesAsync(ctx, poBased, perMt);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, null);

            total.Should().Be(1);
            items[0].QuotesCount.Should().Be(2);
            items[0].Route.Should().Contain("Adilabad Yard");
            items[0].Route.Should().Contain("Dindigul Mill Gate");
            items[0].StatusName.Should().Be("Draft");
        }

        [Fact]
        public async Task NotFoundAsync_And_GetStatusCode_Should_Reflect_State()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (poBased, _, perMt) = await SeedMiscAsync(ctx);
            var id = await SeedRfqWithQuotesAsync(ctx, poBased, perMt);

            (await CreateQueryRepo().NotFoundAsync(id)).Should().BeTrue();
            (await CreateQueryRepo().NotFoundAsync(999999)).Should().BeFalse();
            (await CreateQueryRepo().GetStatusCodeAsync(id)).Should().Be(MiscEnumEntity.Draft);
            (await CreateQueryRepo().GetQuotationCountAsync(id)).Should().Be(2);
        }
    }
}
