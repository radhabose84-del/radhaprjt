using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
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
    public sealed class UserLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UserLookupRepositoryTests(DbFixture fixture)
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

        private UserLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            return new UserLookupRepository(conn, ipMock.Object);
        }

        private async Task<int> EnsureDepartmentAsync()
        {
            await using var ctx = CreateDbContext();
            var existingGrp = await ctx.DepartmentGroup.FirstOrDefaultAsync(g => g.DepartmentGroupCode == "USRGRP");
            int groupId;
            if (existingGrp != null) groupId = existingGrp.Id;
            else
            {
                var grp = new UserManagement.Domain.Entities.DepartmentGroup
                {
                    DepartmentGroupCode = "USRGRP", DepartmentGroupName = "User Group",
                    IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.DepartmentGroup.AddAsync(grp);
                await ctx.SaveChangesAsync();
                groupId = grp.Id;
            }

            var dept = await ctx.Department.FirstOrDefaultAsync(d => d.ShortName == "USRD");
            if (dept != null) return dept.Id;

            dept = new UserManagement.Domain.Entities.Department
            {
                ShortName = "USRD", DeptName = "Users Dept", CompanyId = 1, DepartmentGroupId = groupId,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Department.AddAsync(dept);
            await ctx.SaveChangesAsync();
            return dept.Id;
        }

        private async Task<int> SeedUserAsync(string userName, string firstName = "First", string lastName = "Last", string email = "u@u.com")
        {
            var deptId = await EnsureDepartmentAsync();
            await using var ctx = CreateDbContext();
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = firstName, LastName = lastName, UserName = userName,
                EmailId = email, Mobile = "9999999999", DepartmentId = deptId,
                IsActive = Enums.Status.Active, IsDeleted = (Enums.IsDelete)0
            };
            await ctx.User.AddAsync(user);
            await ctx.SaveChangesAsync();
            return user.UserId;
        }

        private async Task ClearUsersAsync()
        {
            await using var ctx = CreateDbContext();
            var users = await ctx.User.ToListAsync();
            ctx.User.RemoveRange(users);
            await ctx.SaveChangesAsync();
        }

        // --- GetAllUserAsync ---

        [Fact]
        public async Task GetAllUserAsync_Should_Return_Seeded_Users()
        {
            await ClearUsersAsync();
            await SeedUserAsync("alice", "Alice", "Wonder", "alice@test.com");
            await SeedUserAsync("bob", "Bob", "Builder", "bob@test.com");

            var results = await CreateLookupRepo().GetAllUserAsync();

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllUserAsync_Should_Exclude_SoftDeleted()
        {
            await ClearUsersAsync();
            var userId = await SeedUserAsync("alice", "Alice");
            await SeedUserAsync("bob", "Bob");

            await using var ctx = CreateDbContext();
            var user = await ctx.User.FirstAsync(u => u.UserId == userId);
            user.IsDeleted = (Enums.IsDelete)1;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllUserAsync();

            results.Should().HaveCount(1);
            results[0].FirstName.Should().Be("Bob");
        }

        [Fact]
        public async Task GetAllUserAsync_Should_Map_Columns_Correctly()
        {
            await ClearUsersAsync();
            await SeedUserAsync("carol", "Carol", "Summers", "carol@test.com");

            var results = await CreateLookupRepo().GetAllUserAsync();

            results.Should().HaveCount(1);
            var dto = results[0];
            dto.UserName.Should().Be("carol");
            dto.FirstName.Should().Be("Carol");
            dto.LastName.Should().Be("Summers");
            dto.Email.Should().Be("carol@test.com");
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_User()
        {
            await ClearUsersAsync();
            var userId = await SeedUserAsync("daisy", "Daisy");

            var result = await CreateLookupRepo().GetByIdAsync(userId);

            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.UserName.Should().Be("daisy");
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
            await ClearUsersAsync();
            var userId = await SeedUserAsync("emma");

            await using var ctx = CreateDbContext();
            var user = await ctx.User.FirstAsync(u => u.UserId == userId);
            user.IsDeleted = (Enums.IsDelete)1;
            await ctx.SaveChangesAsync();

            var result = await CreateLookupRepo().GetByIdAsync(userId);

            result.Should().BeNull();
        }

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Users()
        {
            await ClearUsersAsync();
            var id1 = await SeedUserAsync("u1", "User1");
            var id2 = await SeedUserAsync("u2", "User2");
            await SeedUserAsync("u3", "User3");

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var results = await CreateLookupRepo().GetByIdsAsync(Array.Empty<int>());

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearUsersAsync();
            var id1 = await SeedUserAsync("active1");
            var id2 = await SeedUserAsync("active2");

            await using var ctx = CreateDbContext();
            var user = await ctx.User.FirstAsync(u => u.UserId == id1);
            user.IsDeleted = (Enums.IsDelete)1;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(1);
            results[0].UserId.Should().Be(id2);
        }
    }
}
