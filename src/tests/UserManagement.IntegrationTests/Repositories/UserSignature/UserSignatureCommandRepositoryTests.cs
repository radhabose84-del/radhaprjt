using Contracts.Interfaces;
using FluentAssertions;
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
    public sealed class UserSignatureCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UserSignatureCommandRepositoryTests(DbFixture fixture)
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

        private UserSignatureCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new UserSignatureCommandRepository(ctx);

        private static async Task<int> SeedUserAsync(ApplicationDbContext ctx, string emailSuffix = "test")
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
                EmailId = $"{emailSuffix}_{Guid.NewGuid():N}@example.com",
                FirstName = "Test",
                LastName = "User",
                UserName = $"testuser_{Guid.NewGuid():N}".Substring(0, 30),
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

        private static UserManagement.Domain.Entities.UserSignature BuildEntity(int userId) =>
            new()
            {
                UserId = userId,
                SignatureImage = new byte[] { 0x89, 0x50, 0x4E, 0x47 },
                FileName = "sig.png",
                ContentType = "image/png",
                FileSizeBytes = 4,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity(userId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity(userId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UserSignature.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.UserId.Should().Be(userId);
            saved.FileName.Should().Be("sig.png");
            saved.ContentType.Should().Be("image/png");
            saved.FileSizeBytes.Should().Be(4);
            saved.SignatureImage.Should().BeEquivalentTo(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity(userId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UserSignature.FirstOrDefaultAsync(x => x.Id == newId);

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
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity(userId));
            ctx.ChangeTracker.Clear();

            var updateModel = new UserManagement.Domain.Entities.UserSignature
            {
                UserId = userId, // should be ignored by UpdateAsync (immutable)
                SignatureImage = new byte[] { 0xAA, 0xBB },
                FileName = "updated.jpg",
                ContentType = "image/jpeg",
                FileSizeBytes = 2,
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(newId, updateModel);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var updated = await ctx.UserSignature.FirstOrDefaultAsync(x => x.Id == newId);

            updated.Should().NotBeNull();
            updated!.FileName.Should().Be("updated.jpg");
            updated.ContentType.Should().Be("image/jpeg");
            updated.SignatureImage.Should().BeEquivalentTo(new byte[] { 0xAA, 0xBB });
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Overwrite_UserId()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId1 = await SeedUserAsync(ctx, emailSuffix: "user1");

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity(userId1));
            ctx.ChangeTracker.Clear();

            // Attempt to change UserId via Update — must be ignored
            var updateModel = new UserManagement.Domain.Entities.UserSignature
            {
                UserId = 99999, // bogus
                SignatureImage = new byte[] { 0x01 },
                FileName = "x.png",
                ContentType = "image/png",
                FileSizeBytes = 1,
                IsActive = Enums.Status.Active
            };
            await repo.UpdateAsync(newId, updateModel);

            ctx.ChangeTracker.Clear();
            var updated = await ctx.UserSignature.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.UserId.Should().Be(userId1); // unchanged
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();

            var repo = CreateRepository(ctx);
            var result = await repo.UpdateAsync(99999, BuildEntity(1));

            result.Should().BeFalse();
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Entity()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity(userId));
            ctx.ChangeTracker.Clear();

            var deleteModel = new UserManagement.Domain.Entities.UserSignature
            {
                IsDeleted = Enums.IsDelete.Deleted
            };

            var result = await repo.DeleteAsync(newId, deleteModel);
            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.UserSignature.FirstOrDefaultAsync(x => x.Id == newId);
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();

            var repo = CreateRepository(ctx);
            var result = await repo.DeleteAsync(99999, new UserManagement.Domain.Entities.UserSignature
            {
                IsDeleted = Enums.IsDelete.Deleted
            });

            result.Should().BeFalse();
        }
    }
}
