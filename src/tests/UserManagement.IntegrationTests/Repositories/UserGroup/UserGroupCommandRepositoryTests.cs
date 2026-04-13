using Contracts.Interfaces;
using FluentAssertions;
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
    public sealed class UserGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UserGroupCommandRepositoryTests(DbFixture fixture)
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

        private UserGroupCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static UserManagement.Domain.Entities.UserGroup BuildUserGroup(
            string code = "TSTGRP",
            string name = "Test Group")
        {
            return new UserManagement.Domain.Entities.UserGroup
            {
                GroupCode = code,
                GroupName = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
        }

        /// <summary>
        /// Soft-delete any test rows to keep tests isolated without breaking
        /// FKs from AppSecurity.Users rows seeded by other integration tests.
        /// </summary>
        private async Task ClearTestUserGroupsAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE AppSecurity.UserGroup SET IsDeleted = 1 WHERE GroupCode LIKE 'TST%' OR GroupCode LIKE 'UPD%' OR GroupCode LIKE 'DEL%'");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id()
        {
            await using var ctx = CreateDbContext();
            await ClearTestUserGroupsAsync(ctx);

            var repo = CreateRepository(ctx);
            var result = await repo.CreateAsync(BuildUserGroup("TSTGRP1", "Test Group 1"));

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTestUserGroupsAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildUserGroup("TSTGRP2", "Test Group 2"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UserGroup.FirstOrDefaultAsync(ug => ug.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.GroupCode.Should().Be("TSTGRP2");
            saved.GroupName.Should().Be("Test Group 2");
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTestUserGroupsAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildUserGroup("TSTGRP3", "Test Group 3"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UserGroup.FirstOrDefaultAsync(ug => ug.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().BeGreaterThan(0);
            saved.CreatedByName.Should().NotBeNullOrEmpty();
            saved.CreatedIP.Should().NotBeNullOrEmpty();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTestUserGroupsAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildUserGroup("UPDGRP1", "Old Name"));
            ctx.ChangeTracker.Clear();

            var updateModel = new UserManagement.Domain.Entities.UserGroup
            {
                GroupCode = "UPDGRP2",
                GroupName = "Updated Name",
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(created.Id, updateModel);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.UserGroup.FirstOrDefaultAsync(ug => ug.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.GroupCode.Should().Be("UPDGRP2");
            saved.GroupName.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var updateModel = new UserManagement.Domain.Entities.UserGroup
            {
                GroupCode = "X",
                GroupName = "X",
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(999999, updateModel);

            result.Should().Be(0);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = CreateDbContext();
            await ClearTestUserGroupsAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = await repo.CreateAsync(BuildUserGroup("DELGRP1", "To Delete"));
            ctx.ChangeTracker.Clear();

            var deleteModel = new UserManagement.Domain.Entities.UserGroup
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteAsync(created.Id, deleteModel);

            result.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.UserGroup.FirstOrDefaultAsync(ug => ug.Id == created.Id);

            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var deleteModel = new UserManagement.Domain.Entities.UserGroup
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteAsync(999999, deleteModel);

            result.Should().Be(0);
        }

        // --- GET BY CODE ---

        [Fact]
        public async Task GetUserGroupByCodeAsync_Should_Return_Matching_Record()
        {
            await using var ctx = CreateDbContext();
            await ClearTestUserGroupsAsync(ctx);

            var repo = CreateRepository(ctx);
            await repo.CreateAsync(BuildUserGroup("TSTGRP4", "Lookup Group"));
            ctx.ChangeTracker.Clear();

            var result = await repo.GetUserGroupByCodeAsync("Lookup Group", "TSTGRP4");

            result.Should().NotBeNull();
            result.GroupCode.Should().Be("TSTGRP4");
            result.GroupName.Should().Be("Lookup Group");
        }

        [Fact]
        public async Task GetUserGroupByCodeAsync_Should_Return_Empty_Entity_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var result = await repo.GetUserGroupByCodeAsync("NonExistent", "NONEXIST");

            result.Should().NotBeNull();
            result.Id.Should().Be(0);
        }
    }
}
