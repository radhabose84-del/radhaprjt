using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.PasswordChange
{
    [Collection("DatabaseCollection")]
    public sealed class PasswordChangeRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PasswordChangeRepositoryTests(DbFixture fixture)
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

        private PasswordChangeRepository CreateRepo(ApplicationDbContext ctx)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PasswordChangeRepository(ctx, ipMock.Object, conn);
        }

        private async Task<int> EnsureDepartmentAsync()
        {
            await using var ctx = CreateDbContext();
            var existingGrp = await ctx.DepartmentGroup.FirstOrDefaultAsync(g => g.DepartmentGroupCode == "PCGRP");
            int groupId;
            if (existingGrp != null) groupId = existingGrp.Id;
            else
            {
                var grp = new UserManagement.Domain.Entities.DepartmentGroup
                {
                    DepartmentGroupCode = "PCGRP", DepartmentGroupName = "PC Group",
                    IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.DepartmentGroup.AddAsync(grp);
                await ctx.SaveChangesAsync();
                groupId = grp.Id;
            }

            var dept = await ctx.Department.FirstOrDefaultAsync(d => d.ShortName == "PCD");
            if (dept != null) return dept.Id;

            dept = new UserManagement.Domain.Entities.Department
            {
                ShortName = "PCD", DeptName = "PC Dept", CompanyId = 1, DepartmentGroupId = groupId,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Department.AddAsync(dept);
            await ctx.SaveChangesAsync();
            return dept.Id;
        }

        private async Task<int> SeedUserAsync(string userName, string passwordHash = "OLDHASH", Enums.FirstTimeUserStatus firstTime = Enums.FirstTimeUserStatus.No)
        {
            var deptId = await EnsureDepartmentAsync();
            await using var ctx = CreateDbContext();
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test", LastName = "User", UserName = userName,
                EmailId = $"{userName}@test.com", Mobile = "9999999999",
                DepartmentId = deptId, PasswordHash = passwordHash,
                IsFirstTimeUser = firstTime,
                IsActive = Enums.Status.Active, IsDeleted = (Enums.IsDelete)0
            };
            await ctx.User.AddAsync(user);
            await ctx.SaveChangesAsync();
            return user.UserId;
        }

        private async Task ClearAsync()
        {
            await using var ctx = CreateDbContext();
            var logs = await ctx.PasswordLogs.ToListAsync();
            ctx.PasswordLogs.RemoveRange(logs);
            await ctx.SaveChangesAsync();

            var users = await ctx.User.ToListAsync();
            ctx.User.RemoveRange(users);
            await ctx.SaveChangesAsync();
        }

        // --- PasswordEncode ---

        [Fact]
        public async Task PasswordEncode_Should_Return_BCrypt_Hash()
        {
            await using var ctx = CreateDbContext();
            var hash = await CreateRepo(ctx).PasswordEncode("mysecret");

            hash.Should().NotBeNullOrEmpty();
            BCrypt.Net.BCrypt.Verify("mysecret", hash).Should().BeTrue();
        }

        [Fact]
        public async Task PasswordEncode_Should_Throw_On_Empty()
        {
            await using var ctx = CreateDbContext();
            Func<Task> act = async () => await CreateRepo(ctx).PasswordEncode("");

            await act.Should().ThrowAsync<ArgumentException>();
        }

        // --- GenerateVerificationCode ---

        [Fact]
        public async Task GenerateVerificationCode_Should_Return_String_Of_Requested_Length()
        {
            await using var ctx = CreateDbContext();
            var code = await CreateRepo(ctx).GenerateVerificationCode(8);

            code.Should().HaveLength(8);
            code.Should().MatchRegex("^[A-Z0-9]+$");
        }

        // --- ChangePassword ---

        [Fact]
        public async Task ChangePassword_Should_Update_User_Hash_And_Log_Entry()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("cp_user", "OLDHASH");
            var newHash = BCrypt.Net.BCrypt.HashPassword("newsecret");
            var log = new PasswordLog { UserId = userId, UserName = "cp_user", PasswordHash = newHash, CreatedAt = DateTime.UtcNow };

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).ChangePassword(userId, "newsecret", log);

            result.Should().BeTrue();

            await using var verifyCtx = CreateDbContext();
            var user = await verifyCtx.User.FirstAsync(u => u.UserId == userId);
            user.PasswordHash.Should().Be(newHash);
            (await verifyCtx.PasswordLogs.AnyAsync(l => l.UserId == userId && l.PasswordHash == newHash)).Should().BeTrue();
        }

        // --- FirstTimeUserChangePassword ---

        [Fact]
        public async Task FirstTimeUserChangePassword_Should_Succeed_When_User_Is_FirstTime()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("ft_user", firstTime: Enums.FirstTimeUserStatus.Yes);
            var newHash = BCrypt.Net.BCrypt.HashPassword("newsecret");
            var log = new PasswordLog { UserId = userId, UserName = "ft_user", PasswordHash = newHash, CreatedAt = DateTime.UtcNow };

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).FirstTimeUserChangePassword(userId, log);

            result.Should().BeTrue();

            await using var verifyCtx = CreateDbContext();
            var user = await verifyCtx.User.FirstAsync(u => u.UserId == userId);
            user.IsFirstTimeUser.Should().Be(Enums.FirstTimeUserStatus.No);
            user.PasswordHash.Should().Be(newHash);
        }

        [Fact]
        public async Task FirstTimeUserChangePassword_Should_Return_False_When_Not_FirstTime()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("normal_user", firstTime: Enums.FirstTimeUserStatus.No);
            var log = new PasswordLog { UserId = userId, UserName = "normal_user", PasswordHash = "H", CreatedAt = DateTime.UtcNow };

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).FirstTimeUserChangePassword(userId, log);

            result.Should().BeFalse();
        }

        // --- ResetUserPassword ---

        [Fact]
        public async Task ResetUserPassword_Should_Return_Success_Message()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("rp_user");
            var log = new PasswordLog { UserId = userId, UserName = "rp_user", PasswordHash = "NEWHASH", CreatedAt = DateTime.UtcNow };

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).ResetUserPassword(userId, log);

            result.Should().Be("Password Reset successfully.");
        }

        [Fact]
        public async Task ResetUserPassword_Should_Return_NotFound_Message_When_User_Missing()
        {
            await ClearAsync();
            var log = new PasswordLog { UserId = 9999, PasswordHash = "X", CreatedAt = DateTime.UtcNow };

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).ResetUserPassword(9999999, log);

            result.Should().Be("Username not found.");
        }

        // --- ValidateFirstTimeUser ---

        [Fact]
        public async Task ValidateFirstTimeUser_Should_Return_True_For_FirstTime_User()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("new_user", firstTime: Enums.FirstTimeUserStatus.Yes);

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).ValidateFirstTimeUser(userId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateFirstTimeUser_Should_Return_False_For_Normal_User()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("veteran_user", firstTime: Enums.FirstTimeUserStatus.No);

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).ValidateFirstTimeUser(userId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ValidateFirstTimeUser_Should_Return_True_When_User_NotFound()
        {
            await ClearAsync();

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).ValidateFirstTimeUser(9999999);

            result.Should().BeTrue();
        }

        // --- GetUserPasswordHashAsync ---

        [Fact]
        public async Task GetUserPasswordHashAsync_Should_Return_Hash()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("hash_user", "SAVED_HASH");

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).GetUserPasswordHashAsync(userId);

            result.Should().Be("SAVED_HASH");
        }

        [Fact]
        public async Task GetUserPasswordHashAsync_Should_Return_Null_When_NotFound()
        {
            await ClearAsync();

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).GetUserPasswordHashAsync(9999999);

            result.Should().BeNull();
        }

        // --- PasswordLog ---

        [Fact]
        public async Task PasswordLog_Should_Persist_Log_Entry()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("log_user");
            var log = new PasswordLog { UserId = userId, UserName = "log_user", PasswordHash = "LOGHASH", CreatedAt = DateTime.UtcNow };

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).PasswordLog(log);

            result.Should().BeTrue();

            await using var verifyCtx = CreateDbContext();
            (await verifyCtx.PasswordLogs.AnyAsync(l => l.UserId == userId && l.PasswordHash == "LOGHASH")).Should().BeTrue();
        }
    }
}
