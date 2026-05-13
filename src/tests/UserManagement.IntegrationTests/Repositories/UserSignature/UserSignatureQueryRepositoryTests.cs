using Contracts.Interfaces;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.UserSignature;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.UserSignature
{
    [Collection("DatabaseCollection")]
    public sealed class UserSignatureQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UserSignatureQueryRepositoryTests(DbFixture fixture)
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

        private UserSignatureQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new UserSignatureQueryRepository(conn);
        }

        private static async Task<int> SeedUserAsync(ApplicationDbContext ctx, string firstName = "Alice", string lastName = "Anderson")
        {
            var deptGroup = new UserManagement.Domain.Entities.DepartmentGroup
            {
                DepartmentGroupCode = $"DG-{Guid.NewGuid():N}".Substring(0, 10),
                DepartmentGroupName = "Test Department Group",
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.DepartmentGroup.AddAsync(deptGroup);
            await ctx.SaveChangesAsync();

            var dept = new UserManagement.Domain.Entities.Department
            {
                ShortName = "TEST",
                DeptName = "Test Department",
                DepartmentGroupId = deptGroup.Id,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            await ctx.Department.AddAsync(dept);
            await ctx.SaveChangesAsync();

            var user = new User
            {
                Id = Guid.NewGuid(),
                EmailId = $"{firstName.ToLower()}.{lastName.ToLower()}_{Guid.NewGuid():N}@example.com",
                FirstName = firstName,
                LastName = lastName,
                UserName = $"u_{Guid.NewGuid():N}".Substring(0, 30),
                IsActive = Enums.Status.Active,
                IsFirstTimeUser = Enums.FirstTimeUserStatus.No,
                IsLocked = 0,
                DepartmentId = dept.Id,
                IsDeleted = Enums.IsDelete.NotDeleted,
                PasswordHash = "test-hash"
            };
            await ctx.User.AddAsync(user);
            await ctx.SaveChangesAsync();
            return user.UserId;
        }

        private static async Task<int> SeedSignatureAsync(
            ApplicationDbContext ctx,
            int userId,
            string fileName = "vishal-1.png",
            Enums.Status isActive = Enums.Status.Active)
        {
            var cmdRepo = new UserSignatureCommandRepository(ctx);
            var entity = new UserManagement.Domain.Entities.UserSignature
            {
                UserId = userId,
                FileName = fileName,
                OriginalFileName = "signature.png",
                FilePath = $"Resources\\UserManagement\\UserSignatures\\{fileName}",
                FileType = "image/png",
                FileSize = 128,
                IsActive = isActive,
                IsDeleted = Enums.IsDelete.NotDeleted
            };
            var newId = await cmdRepo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();
            return newId;
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllUserSignatureAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);
            await SeedSignatureAsync(ctx, userId);

            var (items, total) = await CreateQueryRepo().GetAllUserSignatureAsync(1, 100, null);

            items.Should().HaveCountGreaterThanOrEqualTo(1);
            total.Should().BeGreaterThanOrEqualTo(1);
            items[0].UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetAllUserSignatureAsync_Should_Populate_User_Navigation()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx, firstName: "John", lastName: "Smith");
            await SeedSignatureAsync(ctx, userId);

            var (items, _) = await CreateQueryRepo().GetAllUserSignatureAsync(1, 100, null);

            items[0].User.Should().NotBeNull();
            items[0].User.FirstName.Should().Be("John");
            items[0].User.LastName.Should().Be("Smith");
            items[0].User.EmailId.Should().Contain("john.smith");
        }

        [Fact]
        public async Task GetAllUserSignatureAsync_Should_Exclude_SoftDeleted_Records()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);
            var sigId = await SeedSignatureAsync(ctx, userId);

            await using var ctx2 = CreateDbContext();
            var cmdRepo = new UserSignatureCommandRepository(ctx2);
            await cmdRepo.DeleteAsync(sigId, new UserManagement.Domain.Entities.UserSignature
            {
                IsDeleted = Enums.IsDelete.Deleted
            });

            var (items, total) = await CreateQueryRepo().GetAllUserSignatureAsync(1, 100, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllUserSignatureAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId1 = await SeedUserAsync(ctx, firstName: "Alpha", lastName: "Aaaa");
            var userId2 = await SeedUserAsync(ctx, firstName: "Beta", lastName: "Bbbb");
            await SeedSignatureAsync(ctx, userId1, fileName: "alpha-1.png");
            await SeedSignatureAsync(ctx, userId2, fileName: "beta-2.png");

            var (items, total) = await CreateQueryRepo().GetAllUserSignatureAsync(1, 100, "Alpha");

            items.Should().HaveCount(1);
            total.Should().Be(1);
            items[0].User.FirstName.Should().Be("Alpha");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetUserSignatureByIdAsync_Should_Return_Dto()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);
            var sigId = await SeedSignatureAsync(ctx, userId, "vishal-3.png");

            var result = await CreateQueryRepo().GetUserSignatureByIdAsync(sigId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(sigId);
            result.FileName.Should().Be("vishal-3.png");
        }

        [Fact]
        public async Task GetUserSignatureByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);
            var sigId = await SeedSignatureAsync(ctx, userId);

            await using var ctx2 = CreateDbContext();
            await new UserSignatureCommandRepository(ctx2).DeleteAsync(sigId,
                new UserManagement.Domain.Entities.UserSignature { IsDeleted = Enums.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetUserSignatureByIdAsync(sigId);

            result.Should().BeNull();
        }

        // --- GET BY USER ID ---

        [Fact]
        public async Task GetUserSignatureByUserIdAsync_Should_Return_Active_Signature()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);
            await SeedSignatureAsync(ctx, userId);

            var result = await CreateQueryRepo().GetUserSignatureByUserIdAsync(userId);

            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
        }

        [Fact]
        public async Task GetUserSignatureByUserIdAsync_Should_Exclude_Inactive_Signature()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);
            await SeedSignatureAsync(ctx, userId, isActive: Enums.Status.Inactive);

            var result = await CreateQueryRepo().GetUserSignatureByUserIdAsync(userId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserSignatureByUserIdAsync_Should_Return_Null_When_NoSignature()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();

            var result = await CreateQueryRepo().GetUserSignatureByUserIdAsync(99999);

            result.Should().BeNull();
        }

        // --- HELPER VALIDATIONS ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_NotExists()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(99999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);
            var sigId = await SeedSignatureAsync(ctx, userId);

            var notFound = await CreateQueryRepo().NotFoundAsync(sigId);

            notFound.Should().BeFalse();
        }

        [Fact]
        public async Task UserExistsAsync_Should_Return_True_When_Active_User_Exists()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);

            var exists = await CreateQueryRepo().UserExistsAsync(userId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task UserExistsAsync_Should_Return_False_When_User_NotFound()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();

            var exists = await CreateQueryRepo().UserExistsAsync(99999);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task UserHasSignatureAsync_Should_Return_True_When_Signature_Exists()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);
            await SeedSignatureAsync(ctx, userId);

            var hasSignature = await CreateQueryRepo().UserHasSignatureAsync(userId);

            hasSignature.Should().BeTrue();
        }

        [Fact]
        public async Task UserHasSignatureAsync_Should_Return_False_When_No_Signature()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);

            var hasSignature = await CreateQueryRepo().UserHasSignatureAsync(userId);

            hasSignature.Should().BeFalse();
        }
    }
}
