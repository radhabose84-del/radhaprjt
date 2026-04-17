using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.CommissionSplit;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.CommissionSplit
{
    [Collection("DatabaseCollection")]
    public sealed class CommissionSplitQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public CommissionSplitQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CommissionSplitQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<(List<int> roleIds, int shareTypeId, int firstRoleId)> EnsureMiscAsync(int rolesNeeded = 5)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "CSQ_MT");
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "CSQ_MT", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var roleIds = new List<int>();
            for (int i = 1; i <= rolesNeeded; i++)
            {
                var code = $"CSQ_R{i}";
                var role = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == code);
                if (role == null)
                {
                    role = new SalesManagement.Domain.Entities.MiscMaster
                    {
                        MiscTypeId = t.Id, Code = code, Description = "Role" + i,
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                    };
                    await ctx.MiscMaster.AddAsync(role);
                    await ctx.SaveChangesAsync();
                }
                roleIds.Add(role.Id);
            }
            var share = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "CSQ_S");
            if (share == null)
            {
                share = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "CSQ_S", Description = "Share",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(share);
                await ctx.SaveChangesAsync();
            }
            return (roleIds, share.Id, roleIds[0]);
        }

        private async Task<int> SeedAsync(string name = "Q Split", IsDelete deleted = IsDelete.NotDeleted, Status active = Status.Active, int detailCount = 2)
        {
            var (roleIds, shareTypeId, _) = await EnsureMiscAsync(Math.Max(detailCount, 5));
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new SalesManagement.Domain.Entities.CommissionSplit
            {
                SplitCode = "CSQ" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                SplitName = name,
                IsActive = active, IsDeleted = deleted,
                CommissionSplitDetails = Enumerable.Range(0, detailCount).Select(i =>
                    new SalesManagement.Domain.Entities.CommissionSplitDetail
                    {
                        RoleId = roleIds[i], ShareTypeId = shareTypeId,
                        ShareValue = (i + 1) * 5m,
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                    }).ToList()
            };
            await ctx.CommissionSplit.AddAsync(e);
            await ctx.SaveChangesAsync();
            return e.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("A");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("A", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("AlphaOnlyZZZ");
            await SeedAsync("BetaOnlyZZZ");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "AlphaOnlyZZZ");

            rows.Should().HaveCount(1);
            rows[0].SplitName.Should().Be("AlphaOnlyZZZ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_With_Details()
        {
            await ClearAsync();
            var id = await SeedAsync("WithDetails", detailCount: 3);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Details.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("AutoNameXYZ");

            var result = await CreateRepo().AutocompleteAsync("AutoNameXYZ", CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedAsync("InactiveZZZ", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("InactiveZZZ", CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("DupName");

            var result = await CreateRepo().AlreadyExistsAsync("DupName");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync("SelfName");

            var result = await CreateRepo().AlreadyExistsAsync("SelfName", id);

            result.Should().BeFalse();
        }

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
            var id = await SeedAsync("Exists");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_For_Active()
        {
            var (_, _, roleId) = await EnsureMiscAsync();

            var result = await CreateRepo().MiscMasterExistsAsync(roleId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_NotFound()
        {
            var result = await CreateRepo().MiscMasterExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetMiscMasterCodeAsync_Should_Return_Code()
        {
            var (_, _, roleId) = await EnsureMiscAsync();

            var result = await CreateRepo().GetMiscMasterCodeAsync(roleId);

            result.Should().Be("CSQ_R1");
        }

        [Fact]
        public async Task GetMiscMasterCodeAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetMiscMasterCodeAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_AgentCommissionConfig()
        {
            await ClearAsync();
            var id = await SeedAsync("LinkCheck");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsCommissionSplitLinkedAsync_Should_Return_False_When_No_Active_Config()
        {
            await ClearAsync();
            var id = await SeedAsync("UnlinkCheck");

            var result = await CreateRepo().IsCommissionSplitLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
