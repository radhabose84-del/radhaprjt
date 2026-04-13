using Contracts.Interfaces;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.UserGroup;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.UserGroup
{
    [Collection("DatabaseCollection")]
    public sealed class UserGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UserGroupQueryRepositoryTests(DbFixture fixture)
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

        private UserGroupQueryRepository CreateQueryRepo(string groupCode = "SUPER_ADMIN")
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetGroupCode()).Returns(groupCode);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new UserGroupQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> SeedUserGroupAsync(string code, string name)
        {
            await using var ctx = CreateDbContext();
            var repo = new UserGroupCommandRepository(ctx);
            var created = await repo.CreateAsync(new UserManagement.Domain.Entities.UserGroup
            {
                GroupCode = code,
                GroupName = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            return created.Id;
        }

        private async Task ClearTestUserGroupsAsync()
        {
            await using var ctx = CreateDbContext();
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE AppSecurity.UserGroup SET IsDeleted = 1 WHERE 1=1");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllUserGroupAsync_Should_Return_Seeded_Record()
        {
            await ClearTestUserGroupsAsync();
            await SeedUserGroupAsync("QRYGRP1", "Query Group 1");

            var (items, total) = await CreateQueryRepo().GetAllUserGroupAsync(1, 10, null);

            items.Should().NotBeEmpty();
            total.Should().BeGreaterThan(0);
            items.Should().Contain(g => g.GroupCode == "QRYGRP1");
        }

        [Fact]
        public async Task GetAllUserGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTestUserGroupsAsync();
            var id = await SeedUserGroupAsync("QRYDEL1", "To Delete");

            await using var ctx = CreateDbContext();
            await new UserGroupCommandRepository(ctx).DeleteAsync(id,
                new UserManagement.Domain.Entities.UserGroup { IsDeleted = Enums.IsDelete.Deleted });

            var (items, _) = await CreateQueryRepo().GetAllUserGroupAsync(1, 10, null);

            items.Should().NotContain(g => g.Id == id);
        }

        [Fact]
        public async Task GetAllUserGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTestUserGroupsAsync();
            await SeedUserGroupAsync("QRYALPH", "Alpha Grp");
            await SeedUserGroupAsync("QRYBETA", "Beta Grp");

            var (items, _) = await CreateQueryRepo().GetAllUserGroupAsync(1, 10, "Alpha");

            items.Should().Contain(g => g.GroupName == "Alpha Grp");
            items.Should().NotContain(g => g.GroupName == "Beta Grp");
        }

        [Fact]
        public async Task GetAllUserGroupAsync_Should_Return_Correct_Pagination()
        {
            await ClearTestUserGroupsAsync();
            await SeedUserGroupAsync("QRYPAG1", "Page Grp 1");
            await SeedUserGroupAsync("QRYPAG2", "Page Grp 2");
            await SeedUserGroupAsync("QRYPAG3", "Page Grp 3");

            var (items, total) = await CreateQueryRepo().GetAllUserGroupAsync(1, 2, "QRYPAG");

            items.Should().HaveCountLessOrEqualTo(2);
            total.Should().BeGreaterThanOrEqualTo(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTestUserGroupsAsync();
            var id = await SeedUserGroupAsync("QRYBID1", "By Id Group");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.GroupCode.Should().Be("QRYBID1");
            result.GroupName.Should().Be("By Id Group");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_KeyNotFoundException_When_NotFound()
        {
            await ClearTestUserGroupsAsync();

            var act = async () => await CreateQueryRepo().GetByIdAsync(999999);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        // --- GET USER GROUPS (Autocomplete) ---

        [Fact]
        public async Task GetUserGroups_SuperAdmin_Should_Return_All_Matching()
        {
            await ClearTestUserGroupsAsync();
            await SeedUserGroupAsync("QRYSEARCH1", "Search Target");
            await SeedUserGroupAsync("QRYSEARCH2", "Other Group");

            var result = await CreateQueryRepo("SUPER_ADMIN").GetUserGroups("Search Target");

            result.Should().Contain(g => g.GroupName == "Search Target");
        }

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_No_Linked_Users()
        {
            await ClearTestUserGroupsAsync();
            var id = await SeedUserGroupAsync("QRYVAL1", "Validation Grp");

            var result = await CreateQueryRepo().SoftDeleteValidation(id);

            result.Should().BeFalse();
        }
    }
}
