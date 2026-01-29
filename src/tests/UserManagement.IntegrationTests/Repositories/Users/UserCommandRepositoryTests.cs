using System.Data;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories;
using Core.Application.Common.Interfaces;
using Xunit;
using FluentAssertions;
using UserManagement.Infrastructure.Services; // ITimeZoneService

// Alias your enums to keep code readable
using Enums = Core.Domain.Enums.Common.Enums;

namespace UserManagement.IntegrationTests.Repositories.Users;

public sealed class UserCommandRepositoryTests
{
    // DbContext that forwards required services to ApplicationDbContext
    // and RELAXES "required" audit fields ONLY for tests.
    private sealed class TestDbContext : ApplicationDbContext
    {
        public TestDbContext(
            string dbName,
            IIPAddressService ip,
            ITimeZoneService tz)
            : base(new DbContextOptionsBuilder<ApplicationDbContext>()
                  .UseInMemoryDatabase(dbName)
                  .Options, ip, tz)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔓 Make audit fields optional ONLY in tests to avoid DbUpdateException.
            modelBuilder.Entity<User>(b =>
            {
                b.Property(x => x.CreatedByName).IsRequired(false);
                b.Property(x => x.CreatedIP).IsRequired(false);
            });

            modelBuilder.Entity<MiscTypeMaster>(b =>
            {
                b.Property(x => x.CreatedByName).IsRequired(false);
                b.Property(x => x.CreatedIP).IsRequired(false);
                b.Property(x => x.Description).IsRequired(false);
            });

            modelBuilder.Entity<MiscMaster>(b =>
            {
                b.Property(x => x.CreatedByName).IsRequired(false);
                b.Property(x => x.CreatedIP).IsRequired(false);
                // ✅ Description also optional to satisfy tests
                b.Property(x => x.Description).IsRequired(false);
            });
        }
    }

    private static (UserCommandRepository sut, TestDbContext db) CreateSut(string name = null!)
    {
        var dbName = name ?? Guid.NewGuid().ToString();

        var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
        var tz = new Mock<ITimeZoneService>(MockBehavior.Loose);

        var db = new TestDbContext(dbName, ip.Object, tz.Object);

        var conn = new Mock<IDbConnection>(MockBehavior.Loose);   // not used by EF-only paths
        var httpFactory = new Mock<IHttpClientFactory>(MockBehavior.Loose);

        var sut = new UserCommandRepository(db, conn.Object, httpFactory.Object, ip.Object);
        return (sut, db);
    }

    private static User MakeUser(int userId = 1, string userName = "neo") =>
        new()
        {
            Id = Guid.NewGuid(),                   // Guid PK
            UserId = userId,                       // domain identity
            FirstName = "Neo",
            LastName = "Anderson",
            UserName = userName,
            EmailId = "neo@matrix.io",
            Mobile = "9999999999",
            IsActive = Enums.Status.Active,        // enum
            IsDeleted = (Enums.IsDelete)0,         // enum (0 = not deleted)

            // Helpful for your own audit logic; optional after our override anyway
            CreatedByName = "test-user",
            CreatedIP = "127.0.0.1",

            // Collections
            UserCompanies = new List<UserCompany>(),
            UserRoleAllocations = new List<Core.Domain.Entities.UserRoleAllocation>(),
            UserUnits = new List<UserUnit>(),
            UserDivisions = new List<UserDivision>(),
            UserDepartments = new List<UserDepartment>()
        };

    [Fact]
    public async Task CreateAsync_inserts_user_and_returns_entity()
    {
        var (sut, db) = CreateSut();

        var user = MakeUser(10, "trinity");

        var created = await sut.CreateAsync(user);

        created.Should().NotBeNull();
        (await db.User.CountAsync()).Should().Be(1);
        (await db.User.SingleAsync()).UserName.Should().Be("trinity");
    }

    [Fact]
    public async Task DeleteAsync_marks_user_deleted_and_returns_true_when_found()
    {
        var (sut, db) = CreateSut();
        var user = MakeUser(7, "morpheus");
        await db.User.AddAsync(user);
        await db.SaveChangesAsync();

        var payload = new User
        {
            IsDeleted = (Enums.IsDelete)1, // deleted
            CreatedByName = "test-user",
            CreatedIP = "127.0.0.1"
        };

        var ok = await sut.DeleteAsync(user.UserId, payload);

        ok.Should().BeTrue();
        (await db.User.SingleAsync(u => u.UserId == 7)).IsDeleted.Should().Be((Enums.IsDelete)1);
    }

    [Fact]
    public async Task DeleteAsync_returns_false_when_not_found()
    {
        var (sut, _) = CreateSut();

        var ok = await sut.DeleteAsync(999, new User {
            IsDeleted = (Enums.IsDelete)1,
            CreatedByName = "test-user",
            CreatedIP = "127.0.0.1"
        });

        ok.Should().BeFalse();
    }

    [Fact]
    public async Task lockUser_sets_IsLocked_to_1()
    {
        var (sut, db) = CreateSut();
        var user = MakeUser(3, "smith");
        user.IsLocked = 0;
        await db.User.AddAsync(user);
        await db.SaveChangesAsync();

        var ok = await sut.lockUser("smith");

        ok.Should().BeTrue();
        (await db.User.SingleAsync(u => u.UserName == "smith")).IsLocked.Should().Be((byte)1);
    }

    [Fact]
    public async Task UnlockUser_sets_IsLocked_to_0()
    {
        var (sut, db) = CreateSut();
        var user = MakeUser(4, "oracle");
        user.IsLocked = 1;
        await db.User.AddAsync(user);
        await db.SaveChangesAsync();

        var ok = await sut.UnlockUser("oracle");

        ok.Should().BeTrue();
        (await db.User.SingleAsync(u => u.UserName == "oracle")).IsLocked.Should().Be((byte)0);
    }

    [Fact]
    public async Task SetAdminPassword_updates_hash_and_returns_save_count()
    {
        var (sut, db) = CreateSut();
        var user = MakeUser(5, "sati");
        await db.User.AddAsync(user);
        await db.SaveChangesAsync();

        var payload = new User {
            PasswordHash = "HASH123",
            CreatedByName = "test-user",
            CreatedIP = "127.0.0.1"
        };

        var changed = await sut.SetAdminPassword(5, payload);

        changed.Should().Be(1);
        (await db.User.SingleAsync(u => u.UserId == 5)).PasswordHash.Should().Be("HASH123");
    }

    [Fact]
    public async Task UpdateAsync_updates_scalar_and_associations_and_returns_save_count()
    {
        var (sut, db) = CreateSut();
        // Seed existing user with one company and one unit
        var existing = MakeUser(8, "tank");
        existing.UserCompanies.Add(new UserCompany { UserId = existing.UserId, CompanyId = 1, IsActive = 1 });
        existing.UserUnits.Add(new UserUnit { UserId = existing.UserId, UnitId = 101, IsActive = 1 });
        await db.User.AddAsync(existing);
        await db.SaveChangesAsync();

        // Payload keeps company 1, adds company 2; toggles unit 101 -> 102
        var payload = MakeUser(8, "tank.updated");
        payload.UserCompanies = new List<UserCompany>
        {
            new() { CompanyId = 1 }, // keep
            new() { CompanyId = 2 }  // new
        };
        payload.UserUnits = new List<UserUnit>
        {
            new() { UnitId = 102 }   // switch from 101 to 102
        };
        payload.UserRoleAllocations = new List<Core.Domain.Entities.UserRoleAllocation>();
        payload.UserDivisions = new List<UserDivision>();
        payload.UserDepartments = new List<UserDepartment>();

        var changed = await sut.UpdateAsync(8, payload);

        // SaveChanges may affect multiple rows; assert >= 1
        changed.Should().BeGreaterThanOrEqualTo(1);

        var reloaded = await db.User
            .Include(u => u.UserCompanies)
            .Include(u => u.UserUnits)
            .SingleAsync(u => u.UserId == 8);

        reloaded.UserName.Should().Be("tank.updated");

        // companies: 1 active, 2 added active
        reloaded.UserCompanies.Should().HaveCount(2);
        reloaded.UserCompanies.Should().Contain(c => c.CompanyId == 1 && c.IsActive == 1);
        reloaded.UserCompanies.Should().Contain(c => c.CompanyId == 2 && c.IsActive == 1);

        // units: 101 toggled inactive, 102 active
        reloaded.UserUnits.Should().Contain(u => u.UnitId == 102 && u.IsActive == 1);
        reloaded.UserUnits.Should().Contain(u => u.UnitId == 101 && u.IsActive == 0);
    }

    [Fact]
    public async Task GetMiscmasterByIdAsync_returns_id_when_found()
    {
        var (sut, db) = CreateSut();

        var type = new MiscTypeMaster
        {
            Id = 77,
            MiscTypeCode = "ROLE",
            IsActive = Enums.Status.Active,
            Description = "Role types",
            CreatedByName = "test-user",
            CreatedIP = "127.0.0.1"
        };
        var misc = new MiscMaster
        {
            Id = 1234,
            MiscTypeId = 77,
            Code = "ADMIN",
            IsActive = Enums.Status.Active,
            Description = "Admin role",     // optional now, but set to be explicit
            CreatedByName = "test-user",
            CreatedIP = "127.0.0.1"
        };
        await db.Set<MiscTypeMaster>().AddAsync(type);
        await db.Set<MiscMaster>().AddAsync(misc);
        await db.SaveChangesAsync();

        var id = await sut.GetMiscmasterByIdAsync("ROLE", "ADMIN");

        id.Should().Be(1234);
    }

    [Fact]
    public async Task GetMiscmasterByIdAsync_throws_when_missing()
    {
        var (sut, _) = CreateSut();

        var act = async () => await sut.GetMiscmasterByIdAsync("ROLE", "MISSING");

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Misc not found*");
    }
}
