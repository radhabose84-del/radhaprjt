using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.GlAccountMaster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Entities = FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Application.GlAccountMaster.Services
{
    // US-GL02-10 — propagation/inheritance logic over an in-memory ApplicationDbContext.
    public sealed class GlobalCoaPropagationServiceTests
    {
        private const int TemplateCompany = 100;

        private static DbContextOptions<ApplicationDbContext> NewOptions() =>
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"coa-{Guid.NewGuid()}")
                .EnableSensitiveDataLogging()
                .Options;

        private static ApplicationDbContext NewContext(DbContextOptions<ApplicationDbContext> options)
        {
            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            var tz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            return new ApplicationDbContext(options, ip.Object, tz.Object);
        }

        private static Mock<ICompanyLookup> CompanyLookup() =>
            BuildLookup(new List<CompanyLookupDto>
            {
                new() { CompanyId = TemplateCompany, CompanyName = "Group", EntityId = 1 },
                new() { CompanyId = 200, CompanyName = "Spinning", EntityId = 1 },
                new() { CompanyId = 300, CompanyName = "Processing", EntityId = 1 },
                new() { CompanyId = 999, CompanyName = "OtherEntity", EntityId = 2 },
            });

        private static Mock<ICompanyLookup> BuildLookup(List<CompanyLookupDto> companies)
        {
            var mock = new Mock<ICompanyLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetAllCompanyAsync()).ReturnsAsync(companies);
            return mock;
        }

        private static GlobalCoaPropagationService Sut(ApplicationDbContext ctx, Mock<ICompanyLookup> lookup) =>
            new(ctx, lookup.Object, Options.Create(new MultiCompanyCoaOptions { TemplateCompanyId = TemplateCompany }));

        private static Entities.GlAccountMaster Account(int companyId, string code, bool isGlobal = false,
            bool restricted = false, int? globalAccountId = null, bool localOverride = false, string name = "Acct") =>
            new()
            {
                CompanyId = companyId,
                AccountCode = code,
                AccountName = name,
                AccountTypeId = 1,
                AccountGroupId = 1,
                NormalBalanceId = 1,
                CurrencyTypeId = 1,
                SubLedgerTypeId = 1,
                IsGlobal = isGlobal,
                IsCompanyRestricted = restricted,
                GlobalAccountId = globalAccountId,
                IsLocalOverride = localOverride,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        [Fact]
        public async Task InheritForCompany_CopiesOnlyNonRestrictedGlobals()
        {
            var options = NewOptions();
            int globalA;
            using (var seed = NewContext(options))
            {
                seed.GlAccountMaster.Add(Account(TemplateCompany, "1001", isGlobal: true, name: "Cash"));
                seed.GlAccountMaster.Add(Account(TemplateCompany, "1002", isGlobal: true, restricted: true, name: "Secret"));
                seed.GlAccountMaster.Add(Account(TemplateCompany, "1003", isGlobal: false, name: "Local"));
                await seed.SaveChangesAsync();
                globalA = seed.GlAccountMaster.Single(a => a.AccountCode == "1001").Id;
            }

            int created;
            using (var ctx = NewContext(options))
                created = await Sut(ctx, CompanyLookup()).InheritForCompanyAsync(200, CancellationToken.None);

            created.Should().Be(1);
            using (var verify = NewContext(options))
            {
                var copies = verify.GlAccountMaster.Where(a => a.CompanyId == 200).ToList();
                copies.Should().HaveCount(1);
                copies[0].AccountCode.Should().Be("1001");
                copies[0].GlobalAccountId.Should().Be(globalA);
                copies[0].IsGlobal.Should().BeFalse();
            }
        }

        [Fact]
        public async Task InheritForCompany_IsIdempotent()
        {
            var options = NewOptions();
            using (var seed = NewContext(options))
            {
                seed.GlAccountMaster.Add(Account(TemplateCompany, "1001", isGlobal: true));
                await seed.SaveChangesAsync();
            }

            using (var ctx = NewContext(options))
                await Sut(ctx, CompanyLookup()).InheritForCompanyAsync(200, CancellationToken.None);
            int secondRun;
            using (var ctx = NewContext(options))
                secondRun = await Sut(ctx, CompanyLookup()).InheritForCompanyAsync(200, CancellationToken.None);

            secondRun.Should().Be(0);
            using var verify = NewContext(options);
            verify.GlAccountMaster.Count(a => a.CompanyId == 200).Should().Be(1);
        }

        [Fact]
        public async Task FanOutNewGlobal_CreatesCopyInEachSibling()
        {
            var options = NewOptions();
            int globalA;
            using (var seed = NewContext(options))
            {
                seed.GlAccountMaster.Add(Account(TemplateCompany, "1001", isGlobal: true));
                await seed.SaveChangesAsync();
                globalA = seed.GlAccountMaster.Single().Id;
            }

            int created;
            using (var ctx = NewContext(options))
                created = await Sut(ctx, CompanyLookup()).FanOutNewGlobalAsync(globalA, CancellationToken.None);

            created.Should().Be(2); // companies 200 and 300 (same entity), not 999
            using var verify = NewContext(options);
            verify.GlAccountMaster.Count(a => a.GlobalAccountId == globalA).Should().Be(2);
        }

        [Fact]
        public async Task PropagateUpdate_UpdatesCopies_ButSkipsLocalOverride()
        {
            var options = NewOptions();
            int globalA;
            using (var seed = NewContext(options))
            {
                seed.GlAccountMaster.Add(Account(TemplateCompany, "1001", isGlobal: true, name: "Cash"));
                await seed.SaveChangesAsync();
                globalA = seed.GlAccountMaster.Single().Id;

                seed.GlAccountMaster.Add(Account(200, "1001", globalAccountId: globalA, name: "Cash"));
                seed.GlAccountMaster.Add(Account(300, "1001", globalAccountId: globalA, localOverride: true, name: "Renamed Locally"));
                await seed.SaveChangesAsync();

                var template = seed.GlAccountMaster.Single(a => a.Id == globalA);
                template.AccountName = "Cash & Bank";
                await seed.SaveChangesAsync();
            }

            int updated;
            using (var ctx = NewContext(options))
                updated = await Sut(ctx, CompanyLookup()).PropagateUpdateAsync(globalA, CancellationToken.None);

            updated.Should().Be(1);
            using var verify = NewContext(options);
            verify.GlAccountMaster.Single(a => a.CompanyId == 200).AccountName.Should().Be("Cash & Bank");
            verify.GlAccountMaster.Single(a => a.CompanyId == 300).AccountName.Should().Be("Renamed Locally");
        }

        [Fact]
        public async Task FanOutNewGlobal_NonGlobalAccount_IsNoOp()
        {
            var options = NewOptions();
            int localId;
            using (var seed = NewContext(options))
            {
                seed.GlAccountMaster.Add(Account(TemplateCompany, "1003", isGlobal: false));
                await seed.SaveChangesAsync();
                localId = seed.GlAccountMaster.Single().Id;
            }

            int created;
            using (var ctx = NewContext(options))
                created = await Sut(ctx, CompanyLookup()).FanOutNewGlobalAsync(localId, CancellationToken.None);

            created.Should().Be(0);
        }
    }
}
