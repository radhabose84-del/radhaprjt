using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Repositories.SalesAgreement;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesAgreement
{
    [Collection("DatabaseCollection")]
    public sealed class SalesAgreementQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesAgreementQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // All cross-module lookups mocked Loose; the tests below exercise SQL-only methods
        // (GetAll on empty, NotFound, StatusExists, SalesGroupExists) which never invoke them.
        private SalesAgreementQueryRepository CreateRepo()
        {
            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            return new SalesAgreementQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                new Mock<ICustomerLookup>(MockBehavior.Loose).Object,
                new Mock<IPaymentTermLookup>(MockBehavior.Loose).Object,
                new Mock<IItemLookup>(MockBehavior.Loose).Object,
                new Mock<IUOMLookup>(MockBehavior.Loose).Object,
                new Mock<IUnitLookup>(MockBehavior.Loose).Object,
                ip.Object);
        }

        private async Task<(int sgId, int statusId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "ApprovalStatus");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ApprovalStatus", Description = "ApprovalStatus",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            var status = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == mt.Id && x.Code == "Pending");
            if (status == null)
            {
                status = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = mt.Id, Code = "Pending", Description = "Pending",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(status);
                await ctx.SaveChangesAsync();
            }

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "SAGQORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "SAGQORG", SalesOrganisationName = "SAGQ Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "SAGQ_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "SAGQ_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "SAGQ_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "SAGQ_SG", SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }
            return (sg.Id, status.Id);
        }

        [Fact]
        public async Task GetAllAsync_EmptyData_Should_Return_Empty()
        {
            await _fixture.ClearAllTablesAsync();

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null, null);

            rows.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task StatusExistsAsync_Should_Return_True_For_ApprovalStatus_Row()
        {
            var (_, statusId) = await EnsurePrerequisitesAsync();

            var result = await CreateRepo().StatusExistsAsync(statusId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task StatusExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().StatusExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SalesGroupExistsAsync_Should_Return_True_For_Seeded()
        {
            var (sgId, _) = await EnsurePrerequisitesAsync();

            var result = await CreateRepo().SalesGroupExistsAsync(sgId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesGroupExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().SalesGroupExistsAsync(9999999);
            result.Should().BeFalse();
        }
    }
}
