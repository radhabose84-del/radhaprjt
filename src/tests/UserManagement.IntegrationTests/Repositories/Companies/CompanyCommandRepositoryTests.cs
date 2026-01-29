using System.Threading.Tasks;
using Core.Application.Common.Interfaces;
using Core.Application.Common.Interfaces.ICompany;
using Core.Domain.Entities;
using Core.Domain.Enums.Common;          // for Enums.Status / Enums.IsDelete
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Companies;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Companies
{
    public sealed class CompanyCommandRepositoryTests : IClassFixture<DbFixture>
    {
        private readonly DbFixture _fixture;

        private string ConnectionString => _fixture.ConnectionString;

        public CompanyCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApplicationDbContext CreateDbContext()
        {
            // ✅ Fully configured IP service
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetGroupcode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1"); // IMPORTANT

            // 🔴 These are used by ApplicationDbContext.UpdateIpFields()
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");

            // ✅ Timezone service – ApplicationDbContext uses this too
            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        private ICompanyCommandRepository CreateRepository(ApplicationDbContext ctx)
        {
            var mapper = new Mock<AutoMapper.IMapper>(MockBehavior.Loose).Object;

            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            return new CompanyCommandRepository(ctx, mapper, httpContextAccessor);
        }

        private Company BuildNewCompany()
        {
            return new Company
            {
                CompanyName = "Test Company",
                LegalName = "Test Company Pvt Ltd",
                GstNumber = "GSTTEST1234",
                TIN = "TIN1234",
                TAN = "TAN1234",
                CSTNo = "CST1234",
                YearOfEstablishment = 2020,
                Website = "https://example.com",
                Logo = null,
                EntityId = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = (Enums.IsDelete)0,   // not deleted
                PanNumber = "PAN1234",

                // These are fine; UpdateIpFields may overwrite them anyway
                CreatedBy = 1,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1",

                CompanyAddress = new CompanyAddress
                {
                    AddressLine1 = "Line 1",
                    AddressLine2 = "Line 2",
                    PinCode = "600001",
                    AlternatePhone = "9876543210",
                    Phone = "0123456789",
                    CountryId = 1,
                    StateId = 1,
                    CityId = 1
                },
                CompanyContact = new CompanyContact
                {
                    Name = "Contact Person",
                    Designation = "Manager",
                    Email = "contact@example.com",
                    Phone = "9999999999",
                    Remarks = "Primary contact"
                }
            };
        }

        [Fact]
        public async Task CreateAsync_Should_Insert_Company_With_Address_And_Contact()
        {
            await using var ctx = CreateDbContext();

            ctx.Companies.RemoveRange(ctx.Companies);
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            var company = BuildNewCompany();

            var newId = await repo.CreateAsync(company);

            newId.Should().BeGreaterThan(0);

            var dbCompany = await ctx.Companies
                .Include(c => c.CompanyAddress)
                .Include(c => c.CompanyContact)
                .FirstOrDefaultAsync(c => c.Id == newId);

            dbCompany.Should().NotBeNull();
            dbCompany!.CompanyName.Should().Be("Test Company");
            dbCompany.LegalName.Should().Be("Test Company Pvt Ltd");

            dbCompany.IsActive.Should().Be(Enums.Status.Active);
            dbCompany.IsDeleted.Should().Be((Enums.IsDelete)0);

            dbCompany.CompanyAddress.Should().NotBeNull();
            dbCompany.CompanyAddress!.AddressLine1.Should().Be("Line 1");
            dbCompany.CompanyAddress.PinCode.Should().Be("600001");

            dbCompany.CompanyContact.Should().NotBeNull();
            dbCompany.CompanyContact!.Name.Should().Be("Contact Person");
            dbCompany.CompanyContact.Email.Should().Be("contact@example.com");
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Company_And_Nested_Entities()
        {
            await using var ctx = CreateDbContext();

            ctx.Companies.RemoveRange(ctx.Companies);
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);

            var company = BuildNewCompany();
            var id = await repo.CreateAsync(company);

            ctx.ChangeTracker.Clear();

            var updated = BuildNewCompany();
            updated.CompanyName = "Updated Company Name";
            updated.LegalName = "Updated Legal Name";
            updated.GstNumber = "GSTUPDATED1234";
            updated.CompanyAddress.AddressLine1 = "Updated Line 1";
            updated.CompanyContact.Name = "Updated Contact";

            var result = await repo.UpdateAsync(id, updated);

            result.Should().BeTrue();

            var dbCompany = await ctx.Companies
                .Include(c => c.CompanyAddress)
                .Include(c => c.CompanyContact)
                .FirstOrDefaultAsync(c => c.Id == id);

            dbCompany.Should().NotBeNull();
            dbCompany!.CompanyName.Should().Be("Updated Company Name");
            dbCompany.LegalName.Should().Be("Updated Legal Name");
            dbCompany.GstNumber.Should().Be("GSTUPDATED1234");

            dbCompany.IsActive.Should().Be(Enums.Status.Active);
            dbCompany.IsDeleted.Should().Be((Enums.IsDelete)0);

            dbCompany.CompanyAddress!.AddressLine1.Should().Be("Updated Line 1");
            dbCompany.CompanyContact!.Name.Should().Be("Updated Contact");
        }

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Company()
        {
            await using var ctx = CreateDbContext();

            ctx.Companies.RemoveRange(ctx.Companies);
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            var company = BuildNewCompany();

            var id = await repo.CreateAsync(company);

            var deleteModel = new Company
            {
                IsDeleted = (Enums.IsDelete)1   // mark as deleted
            };

            var result = await repo.DeleteAsync(id, deleteModel);

            result.Should().BeTrue();

            var dbCompany = await ctx.Companies
                .FirstOrDefaultAsync(c => c.Id == id);

            dbCompany.Should().NotBeNull();
            dbCompany!.IsDeleted.Should().Be((Enums.IsDelete)1);
        }
    }
}
