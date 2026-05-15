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

        private static UserManagement.Domain.Entities.UserSignature BuildEntity(int userId, string fileName = "vishal-1.png") =>
            new()
            {
                UserId = userId,
                FileName = fileName,
                OriginalFileName = "signature.png",
                FilePath = $"Resources\\UserManagement\\UserSignatures\\{fileName}",
                FileType = "image/png",
                FileSize = 128,
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
            var newId = await repo.CreateAsync(BuildEntity(userId, "vishal-2.png"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UserSignature.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.UserId.Should().Be(userId);
            saved.FileName.Should().Be("vishal-2.png");
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
        public async Task UpdateAsync_Should_Persist_FileName_Change()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity(userId, "old-name.png"));
            ctx.ChangeTracker.Clear();

            var updateModel = new UserManagement.Domain.Entities.UserSignature
            {
                FileName = "new-name.jpg",
                OriginalFileName = "newsig.jpg",
                FilePath = "Resources\\UserManagement\\UserSignatures\\new-name.jpg",
                FileType = "image/jpeg",
                FileSize = 256,
                IsActive = Enums.Status.Active
            };

            var result = await repo.UpdateAsync(newId, updateModel);
            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var updated = await ctx.UserSignature.FirstOrDefaultAsync(x => x.Id == newId);

            updated!.FileName.Should().Be("new-name.jpg");
            updated.OriginalFileName.Should().Be("newsig.jpg");
            updated.FileType.Should().Be("image/jpeg");
            updated.FileSize.Should().Be(256);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Overwrite_UserId()
        {
            await using var ctx = CreateDbContext();
            await _fixture.ClearAllTablesAsync();
            var userId = await SeedUserAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity(userId));
            ctx.ChangeTracker.Clear();

            var updateModel = new UserManagement.Domain.Entities.UserSignature
            {
                UserId = 99999, // bogus — must be ignored
                FileName = "x.png",
                OriginalFileName = "x.png",
                FilePath = "Resources\\UserManagement\\UserSignatures\\x.png",
                FileType = "image/png",
                FileSize = 64,
                IsActive = Enums.Status.Active
            };
            await repo.UpdateAsync(newId, updateModel);

            ctx.ChangeTracker.Clear();
            var updated = await ctx.UserSignature.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.UserId.Should().Be(userId); // unchanged
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
