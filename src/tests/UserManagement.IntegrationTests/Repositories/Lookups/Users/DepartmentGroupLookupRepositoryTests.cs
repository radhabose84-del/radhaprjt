using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Lookups.Users
{
    [Collection("DatabaseCollection")]
    public sealed class DepartmentGroupLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DepartmentGroupLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApplicationDbContext CreateDbContext()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        private DepartmentGroupLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DepartmentGroupLookupRepository(conn);
        }

        private async Task<int> SeedGroupAsync(string code = "LKPGRP", string name = "Lookup Group")
        {
            await using var ctx = CreateDbContext();
            var group = new UserManagement.Domain.Entities.DepartmentGroup
            {
                DepartmentGroupCode = code,
                DepartmentGroupName = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.DepartmentGroup.AddAsync(group);
            await ctx.SaveChangesAsync();
            return group.Id;
        }

        private async Task ClearTestGroupsAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_Group()
        {
            await ClearTestGroupsAsync();
            var id = await SeedGroupAsync("LKPG1", "Lookup Grp 1");

            var result = await CreateLookupRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.DepartmentGroupId.Should().Be(id);
            result.DepartmentGroupName.Should().Be("Lookup Grp 1");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateLookupRepo().GetByIdAsync(9999999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTestGroupsAsync();
            var id = await SeedGroupAsync("LKPG2", "Lookup Grp 2");

            await using var ctx = CreateDbContext();
            var grp = await ctx.DepartmentGroup.FirstAsync(g => g.Id == id);
            grp.IsDeleted = Enums.IsDelete.Deleted;
            await ctx.SaveChangesAsync();

            var result = await CreateLookupRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Inactive_When_NotDeleted()
        {
            await ClearTestGroupsAsync();
            var id = await SeedGroupAsync("LKPG3", "Lookup Grp 3");

            await using var ctx = CreateDbContext();
            var grp = await ctx.DepartmentGroup.FirstAsync(g => g.Id == id);
            grp.IsActive = Enums.Status.Inactive;
            await ctx.SaveChangesAsync();

            var result = await CreateLookupRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.DepartmentGroupId.Should().Be(id);
        }
    }
}
