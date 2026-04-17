using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Module;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Module
{
    [Collection("DatabaseCollection")]
    public sealed class ModuleCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ModuleCommandRepositoryTests(DbFixture fixture)
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

        private ModuleCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new ModuleCommandRepository(ctx);

        private async Task ClearTestDataAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- ADD MODULE ---

        [Fact]
        public async Task AddModuleAsync_Should_Persist_Module()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var module = new Modules
            {
                ModuleName = "TestModule_CMD_Alpha",
                IsDeleted = false
            };

            await repo.AddModuleAsync(module);
            await repo.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Modules.FirstOrDefaultAsync(m => m.ModuleName == "TestModule_CMD_Alpha");

            saved.Should().NotBeNull();
            saved!.ModuleName.Should().Be("TestModule_CMD_Alpha");
        }

        [Fact]
        public async Task AddModuleAsync_Should_Throw_When_Duplicate_Name()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var module = new Modules { ModuleName = "TestModule_CMD_Dup", IsDeleted = false };
            await repo.AddModuleAsync(module);
            await repo.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            await using var ctx2 = CreateDbContext();
            var repo2 = new ModuleCommandRepository(ctx2);

            Func<Task> act = async () =>
            {
                var dupModule = new Modules { ModuleName = "TestModule_CMD_Dup", IsDeleted = false };
                await repo2.AddModuleAsync(dupModule);
                await repo2.SaveChangesAsync();
            };

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // --- UPDATE MODULE ---

        [Fact]
        public async Task UpdateModuleAsync_Should_Persist_Changes()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var module = new Modules { ModuleName = "TestModule_CMD_Update", IsDeleted = false };
            await repo.AddModuleAsync(module);
            await repo.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var updateModule = new Modules
            {
                Id = module.Id,
                ModuleName = "TestModule_CMD_Updated"
            };

            await using var ctx2 = CreateDbContext();
            var repo2 = new ModuleCommandRepository(ctx2);
            var result = await repo2.UpdateModuleAsync(updateModule);

            result.Should().BeTrue();

            ctx2.ChangeTracker.Clear();
            var updated = await ctx2.Modules.FirstOrDefaultAsync(m => m.Id == module.Id);
            updated.Should().NotBeNull();
            updated!.ModuleName.Should().Be("TestModule_CMD_Updated");
        }

        // --- DELETE MODULE ---

        [Fact]
        public async Task DeleteModuleAsync_Should_Soft_Delete_Module()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);

            var repo = CreateRepository(ctx);
            var module = new Modules { ModuleName = "TestModule_CMD_Delete", IsDeleted = false };
            await repo.AddModuleAsync(module);
            await repo.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            await using var ctx2 = CreateDbContext();
            var repo2 = new ModuleCommandRepository(ctx2);
            await repo2.DeleteModuleAsync(module.Id);
            ctx2.ChangeTracker.Clear();

            var deleted = await ctx2.Modules.FirstOrDefaultAsync(m => m.Id == module.Id);
            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().BeTrue();
        }
    }
}
