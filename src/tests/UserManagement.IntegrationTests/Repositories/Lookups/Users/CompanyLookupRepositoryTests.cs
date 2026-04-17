using AutoMapper;
using Contracts.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Companies;
using UserManagement.Infrastructure.Repositories.Lookups.Users;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Lookups.Users
{
    [Collection("DatabaseCollection")]
    public sealed class CompanyLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CompanyLookupRepositoryTests(DbFixture fixture)
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

        private CompanyLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CompanyLookupRepository(conn);
        }

        private static Company BuildCompany(string name, string legalName, string gst, string pan) =>
            new()
            {
                CompanyName = name,
                LegalName = legalName,
                GstNumber = gst,
                TIN = "TIN",
                TAN = "TAN",
                CSTNo = "CST",
                YearOfEstablishment = 2020,
                Website = "https://example.com",
                Logo = "logo.png",
                EntityId = 1,
                PanNumber = pan,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted,
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
                    Name = "Contact",
                    Designation = "Manager",
                    Email = "contact@example.com",
                    Phone = "9999999999",
                    Remarks = "Primary"
                }
            };

        private async Task<int> SeedCompanyAsync(string name, string legalName, string gst, string pan)
        {
            await using var ctx = CreateDbContext();
            var mapperMock = new Mock<IMapper>(MockBehavior.Loose);
            var httpContextMock = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
            var repo = new CompanyCommandRepository(ctx, mapperMock.Object, httpContextMock.Object);
            return await repo.CreateAsync(BuildCompany(name, legalName, gst, pan));
        }

        private async Task ClearTestCompaniesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetAllCompanyAsync ---

        [Fact]
        public async Task GetAllCompanyAsync_Should_Return_Seeded_Record()
        {
            await ClearTestCompaniesAsync();
            await SeedCompanyAsync("LookupCo A", "LookupCo A Pvt", "GSTLA", "PANLA");

            var results = await CreateLookupRepo().GetAllCompanyAsync();

            results.Should().NotBeEmpty();
            results.Should().Contain(c => c.CompanyName == "LookupCo A");
        }

        [Fact]
        public async Task GetAllCompanyAsync_Should_Map_Columns_Correctly()
        {
            await ClearTestCompaniesAsync();
            await SeedCompanyAsync("MapCo", "MapCo Pvt", "GSTMAP", "PANMAP");

            var results = await CreateLookupRepo().GetAllCompanyAsync();

            var dto = results.First(c => c.CompanyName == "MapCo");
            dto.LegalName.Should().Be("MapCo Pvt");
            dto.GstNumber.Should().Be("GSTMAP");
            dto.TinNumber.Should().Be("TIN");
            dto.TanNumber.Should().Be("TAN");
            dto.CstNumber.Should().Be("CST");
            dto.EntityId.Should().Be(1);
        }

        [Fact]
        public async Task GetAllCompanyAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTestCompaniesAsync();
            await SeedCompanyAsync("DelCo", "DelCo Pvt", "GSTDEL", "PANDEL");
            await ClearTestCompaniesAsync();

            var results = await CreateLookupRepo().GetAllCompanyAsync();

            results.Should().NotContain(c => c.CompanyName == "DelCo");
        }

        [Fact]
        public async Task GetAllCompanyAsync_Should_Include_Inactive_Companies()
        {
            await ClearTestCompaniesAsync();
            var id = await SeedCompanyAsync("InactiveCo", "InactiveCo Pvt", "GSTINA", "PANINA");

            await using var ctx = CreateDbContext();
            var entity = await ctx.Companies.FirstAsync(c => c.Id == id);
            entity.IsActive = Enums.Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateLookupRepo().GetAllCompanyAsync();

            results.Should().Contain(c => c.CompanyName == "InactiveCo");
        }

        [Fact]
        public async Task GetAllCompanyAsync_Should_Order_By_CompanyName_Ascending()
        {
            await ClearTestCompaniesAsync();
            await SeedCompanyAsync("ZetaCo", "ZetaCo Pvt", "GSTZ", "PANZ");
            await SeedCompanyAsync("AlphaCo", "AlphaCo Pvt", "GSTA", "PANA");
            await SeedCompanyAsync("BetaCo", "BetaCo Pvt", "GSTB", "PANB");

            var results = await CreateLookupRepo().GetAllCompanyAsync();
            var names = results.Select(c => c.CompanyName).ToList();

            names.IndexOf("AlphaCo").Should().BeLessThan(names.IndexOf("BetaCo"));
            names.IndexOf("BetaCo").Should().BeLessThan(names.IndexOf("ZetaCo"));
        }

        [Fact]
        public async Task GetAllCompanyAsync_EmptyTable_ReturnsEmptyList()
        {
            await ClearTestCompaniesAsync();

            var results = await CreateLookupRepo().GetAllCompanyAsync();

            results.Should().NotBeNull();
            results.Should().BeEmpty();
        }
    }
}
