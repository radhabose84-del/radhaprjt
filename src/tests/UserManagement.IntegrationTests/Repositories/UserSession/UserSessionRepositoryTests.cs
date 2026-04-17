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

namespace UserManagement.IntegrationTests.Repositories.UserSession
{
    [Collection("DatabaseCollection")]
    public sealed class UserSessionRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UserSessionRepositoryTests(DbFixture fixture)
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

        private UserSessionRepository CreateRepo(ApplicationDbContext ctx, DateTime? currentTime = null)
        {
            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(currentTime ?? DateTime.UtcNow);
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new UserSessionRepository(ctx, tzMock.Object, conn);
        }

        private async Task<int> EnsureDepartmentAsync()
        {
            await using var ctx = CreateDbContext();
            var existingGrp = await ctx.DepartmentGroup.FirstOrDefaultAsync(g => g.DepartmentGroupCode == "USGRP");
            int groupId;
            if (existingGrp != null) groupId = existingGrp.Id;
            else
            {
                var grp = new UserManagement.Domain.Entities.DepartmentGroup
                {
                    DepartmentGroupCode = "USGRP", DepartmentGroupName = "US Group",
                    IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.DepartmentGroup.AddAsync(grp);
                await ctx.SaveChangesAsync();
                groupId = grp.Id;
            }

            var dept = await ctx.Department.FirstOrDefaultAsync(d => d.ShortName == "USD");
            if (dept != null) return dept.Id;

            dept = new UserManagement.Domain.Entities.Department
            {
                ShortName = "USD", DeptName = "US Dept", CompanyId = 1, DepartmentGroupId = groupId,
                IsActive = Enums.Status.Active, IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Department.AddAsync(dept);
            await ctx.SaveChangesAsync();
            return dept.Id;
        }

        private async Task<int> SeedUserAsync(string userName)
        {
            var deptId = await EnsureDepartmentAsync();
            await using var ctx = CreateDbContext();
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "US", LastName = "User", UserName = userName,
                EmailId = $"{userName}@test.com", Mobile = "9999999999", DepartmentId = deptId,
                IsActive = Enums.Status.Active, IsDeleted = (Enums.IsDelete)0
            };
            await ctx.User.AddAsync(user);
            await ctx.SaveChangesAsync();
            return user.UserId;
        }

        private static UserSessions BuildSession(int userId, string jwtId = "jwt-xyz", byte isActive = 1, DateTime? expires = null) =>
            new()
            {
                UserId = userId,
                JwtId = jwtId,
                BrowserInfo = "test-browser",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expires ?? DateTime.UtcNow.AddHours(1),
                IsActive = isActive,
                LastActivity = DateTime.UtcNow
            };

        private async Task ClearAsync()
        {
            await using var ctx = CreateDbContext();
            var sessions = await ctx.UserSession.ToListAsync();
            ctx.UserSession.RemoveRange(sessions);
            await ctx.SaveChangesAsync();

            var users = await ctx.User.ToListAsync();
            ctx.User.RemoveRange(users);
            await ctx.SaveChangesAsync();
        }

        // --- AddSessionAsync / GetSessionByJwtIdAsync ---

        [Fact]
        public async Task AddSessionAsync_Should_Persist_Session()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("add_user");
            var session = BuildSession(userId, "jwt-add");

            await using var ctx = CreateDbContext();
            await CreateRepo(ctx).AddSessionAsync(session);

            await using var verifyCtx = CreateDbContext();
            var saved = await verifyCtx.UserSession.FirstOrDefaultAsync(s => s.JwtId == "jwt-add");
            saved.Should().NotBeNull();
            saved!.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetSessionByJwtIdAsync_Should_Return_Matching_Session()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("jwt_user");
            var session = BuildSession(userId, "unique-jwt-1");

            await using var ctx = CreateDbContext();
            await CreateRepo(ctx).AddSessionAsync(session);

            await using var readCtx = CreateDbContext();
            var result = await CreateRepo(readCtx).GetSessionByJwtIdAsync("unique-jwt-1");

            result.Should().NotBeNull();
            result.JwtId.Should().Be("unique-jwt-1");
        }

        [Fact]
        public async Task GetSessionByJwtIdAsync_Should_Return_Empty_Session_When_NotFound()
        {
            await ClearAsync();

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).GetSessionByJwtIdAsync("non-existent-jwt");

            result.Should().NotBeNull();
            result.Id.Should().Be(0);
        }

        // --- UpdateSessionAsync ---

        [Fact]
        public async Task UpdateSessionAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("upd_user");
            var session = BuildSession(userId, "upd-jwt");
            await using var ctx = CreateDbContext();
            await CreateRepo(ctx).AddSessionAsync(session);

            await using var upCtx = CreateDbContext();
            var saved = await upCtx.UserSession.FirstAsync(s => s.JwtId == "upd-jwt");
            saved.BrowserInfo = "updated-browser";
            await CreateRepo(upCtx).UpdateSessionAsync(saved);

            await using var verifyCtx = CreateDbContext();
            var reread = await verifyCtx.UserSession.FirstAsync(s => s.JwtId == "upd-jwt");
            reread.BrowserInfo.Should().Be("updated-browser");
        }

        // --- DeactivateUserSessionsAsync ---

        [Fact]
        public async Task DeactivateUserSessionsAsync_Should_Set_IsActive_Zero()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("deact_user");
            await using var ctx = CreateDbContext();
            await CreateRepo(ctx).AddSessionAsync(BuildSession(userId, "deact-1"));
            await CreateRepo(ctx).AddSessionAsync(BuildSession(userId, "deact-2"));

            await using var deactCtx = CreateDbContext();
            await CreateRepo(deactCtx).DeactivateUserSessionsAsync(userId);

            await using var verifyCtx = CreateDbContext();
            var sessions = await verifyCtx.UserSession.Where(s => s.UserId == userId).ToListAsync();
            sessions.Should().HaveCount(2);
            sessions.Should().OnlyContain(s => s.IsActive == 0);
        }

        // --- GetSessionByUserIdAsync ---

        [Fact]
        public async Task GetSessionByUserIdAsync_Should_Return_Active_NonExpired_Session()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("gs_user");
            await using var ctx = CreateDbContext();
            await CreateRepo(ctx).AddSessionAsync(BuildSession(userId, "active-jwt", 1, DateTime.UtcNow.AddHours(1)));

            await using var readCtx = CreateDbContext();
            var result = await CreateRepo(readCtx).GetSessionByUserIdAsync(userId);

            result.Should().NotBeNull();
            result!.JwtId.Should().Be("active-jwt");
        }

        [Fact]
        public async Task GetSessionByUserIdAsync_Should_Return_Null_For_Expired_Session()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("exp_user");
            await using var ctx = CreateDbContext();
            await CreateRepo(ctx).AddSessionAsync(BuildSession(userId, "exp-jwt", 1, DateTime.UtcNow.AddHours(-1)));

            await using var readCtx = CreateDbContext();
            var result = await CreateRepo(readCtx).GetSessionByUserIdAsync(userId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetSessionByUserIdAsync_Should_Return_Null_For_Inactive_Session()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("ia_user");
            await using var ctx = CreateDbContext();
            await CreateRepo(ctx).AddSessionAsync(BuildSession(userId, "ia-jwt", isActive: 0));

            await using var readCtx = CreateDbContext();
            var result = await CreateRepo(readCtx).GetSessionByUserIdAsync(userId);

            result.Should().BeNull();
        }

        // --- ExpireTokenAsync ---

        [Fact]
        public async Task ExpireTokenAsync_Should_Mark_Session_Inactive()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("exp_token_user");
            await using var ctx = CreateDbContext();
            await CreateRepo(ctx).AddSessionAsync(BuildSession(userId, "to-expire"));

            await using var expCtx = CreateDbContext();
            await CreateRepo(expCtx).ExpireTokenAsync("to-expire");

            await using var verifyCtx = CreateDbContext();
            var session = await verifyCtx.UserSession.FirstAsync(s => s.JwtId == "to-expire");
            session.IsActive.Should().Be(0);
        }

        [Fact]
        public async Task ExpireTokenAsync_Should_NoOp_When_Session_NotFound()
        {
            await ClearAsync();

            await using var ctx = CreateDbContext();
            var act = () => CreateRepo(ctx).ExpireTokenAsync("non-existent");

            await act.Should().NotThrowAsync();
        }

        // --- DeactivateExpiredSessionsAsync ---

        [Fact]
        public async Task DeactivateExpiredSessionsAsync_Should_Deactivate_All_Expired()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("bulk_user");
            await using var ctx = CreateDbContext();
            await CreateRepo(ctx).AddSessionAsync(BuildSession(userId, "active", 1, DateTime.UtcNow.AddHours(1)));
            await CreateRepo(ctx).AddSessionAsync(BuildSession(userId, "expired-1", 1, DateTime.UtcNow.AddHours(-1)));
            await CreateRepo(ctx).AddSessionAsync(BuildSession(userId, "expired-2", 1, DateTime.UtcNow.AddHours(-2)));

            await using var deactCtx = CreateDbContext();
            await CreateRepo(deactCtx).DeactivateExpiredSessionsAsync();

            await using var verifyCtx = CreateDbContext();
            var sessions = await verifyCtx.UserSession.Where(s => s.UserId == userId).ToListAsync();
            sessions.Single(s => s.JwtId == "active").IsActive.Should().Be(1);
            sessions.Single(s => s.JwtId == "expired-1").IsActive.Should().Be(0);
            sessions.Single(s => s.JwtId == "expired-2").IsActive.Should().Be(0);
        }

        // --- ValidateUserSession ---

        [Fact]
        public async Task ValidateUserSession_Should_Return_True_For_Active_NonExpired()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("vs_user");
            await using var ctx = CreateDbContext();
            await CreateRepo(ctx).AddSessionAsync(BuildSession(userId, "vs-jwt", 1, DateTime.UtcNow.AddHours(1)));

            await using var valCtx = CreateDbContext();
            var result = await CreateRepo(valCtx).ValidateUserSession("vs_user");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ValidateUserSession_Should_Return_False_When_NoActiveSession()
        {
            await ClearAsync();
            await SeedUserAsync("no_sess_user");

            await using var ctx = CreateDbContext();
            var result = await CreateRepo(ctx).ValidateUserSession("no_sess_user");

            result.Should().BeFalse();
        }

        // --- DeactivateUserSessionsByUsername ---

        [Fact]
        public async Task DeactivateUserSessionsByUsername_Should_Deactivate_Active_Sessions()
        {
            await ClearAsync();
            var userId = await SeedUserAsync("byname_user");
            await using var ctx = CreateDbContext();
            await CreateRepo(ctx).AddSessionAsync(BuildSession(userId, "bn-1"));
            await CreateRepo(ctx).AddSessionAsync(BuildSession(userId, "bn-2"));

            await using var deactCtx = CreateDbContext();
            var result = await CreateRepo(deactCtx).DeactivateUserSessionsByUsername("byname_user");

            result.Should().BeTrue();

            await using var verifyCtx = CreateDbContext();
            var sessions = await verifyCtx.UserSession.Where(s => s.UserId == userId).ToListAsync();
            sessions.Should().OnlyContain(s => s.IsActive == 0);
        }
    }
}
