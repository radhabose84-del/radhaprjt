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
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Companies
{
    [Collection("DatabaseCollection")]
    public sealed class CompanyQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CompanyQueryRepositoryTests(DbFixture fixture)
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

        private CompanyQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CompanyQueryRepository(conn, ipMock.Object);
        }

        private static Company BuildCompany(string name, string legalName, string gst, string pan)
        {
            return new Company
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
        }

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

        // --- GET ALL ---

        [Fact]
        public async Task GetAllCompaniesAsync_Should_Return_Seeded_Record()
        {
            await ClearTestCompaniesAsync();
            await SeedCompanyAsync("QryCo A", "QryCo A Pvt", "GSTQA", "PANQA");

            var (items, total) = await CreateQueryRepo().GetAllCompaniesAsync(1, 10, null);

            items.Should().NotBeEmpty();
            total.Should().BeGreaterThan(0);
            items.Should().Contain(c => c.CompanyName == "QryCo A");
        }

        [Fact]
        public async Task GetAllCompaniesAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTestCompaniesAsync();
            var id = await SeedCompanyAsync("QryCo Del", "QryCo Del Pvt", "GSTQDEL", "PANQDEL");

            await using var ctx = CreateDbContext();
            var mapperMock = new Mock<IMapper>(MockBehavior.Loose);
            var httpContextMock = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
            var cmdRepo = new CompanyCommandRepository(ctx, mapperMock.Object, httpContextMock.Object);
            await cmdRepo.DeleteAsync(id, new Company { IsDeleted = Enums.IsDelete.Deleted });

            var (items, _) = await CreateQueryRepo().GetAllCompaniesAsync(1, 10, null);

            items.Should().NotContain(c => c.Id == id);
        }

        [Fact]
        public async Task GetAllCompaniesAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTestCompaniesAsync();
            await SeedCompanyAsync("Alpha QryCo", "Alpha QryCo Pvt", "GSTALPH", "PANALPH");
            await SeedCompanyAsync("Beta QryCo", "Beta QryCo Pvt", "GSTBETA", "PANBETA");

            var (items, _) = await CreateQueryRepo().GetAllCompaniesAsync(1, 10, "Alpha");

            items.Should().Contain(c => c.CompanyName == "Alpha QryCo");
            items.Should().NotContain(c => c.CompanyName == "Beta QryCo");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Record_With_Address_And_Contact()
        {
            await ClearTestCompaniesAsync();
            var id = await SeedCompanyAsync("ById QryCo", "ById QryCo Pvt", "GSTBID", "PANBID");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.CompanyName.Should().Be("ById QryCo");
            result.CompanyAddress.Should().NotBeNull();
            result.CompanyContact.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTestCompaniesAsync();
            var id = await SeedCompanyAsync("Deleted QryCo", "Deleted QryCo Pvt", "GSTDEL", "PANDEL");

            await using var ctx = CreateDbContext();
            var mapperMock = new Mock<IMapper>(MockBehavior.Loose);
            var httpContextMock = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
            var cmdRepo = new CompanyCommandRepository(ctx, mapperMock.Object, httpContextMock.Object);
            await cmdRepo.DeleteAsync(id, new Company { IsDeleted = Enums.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GET BY COMPANY NAME ---

        [Fact]
        public async Task GetByCompanynameAsync_Should_Return_Matching_Record()
        {
            await ClearTestCompaniesAsync();
            await SeedCompanyAsync("Named QryCo", "Named QryCo Pvt", "GSTNAM", "PANNAM");

            var result = await CreateQueryRepo().GetByCompanynameAsync("Named QryCo");

            result.Should().NotBeNull();
            result!.CompanyName.Should().Be("Named QryCo");
        }

        [Fact]
        public async Task GetByCompanynameAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await ClearTestCompaniesAsync();
            var id = await SeedCompanyAsync("Self QryCo", "Self QryCo Pvt", "GSTSLF", "PANSLF");

            var result = await CreateQueryRepo().GetByCompanynameAsync("Self QryCo", id);

            result.Should().BeNull();
        }

        // --- CompanyExistsAsync ---

        [Fact]
        public async Task CompanyExistsAsync_Should_Return_True_For_Existing()
        {
            await ClearTestCompaniesAsync();
            await SeedCompanyAsync("Exists QryCo", "Exists QryCo Pvt", "GSTEX", "PANEX");

            var exists = await CreateQueryRepo().CompanyExistsAsync("Exists QryCo");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CompanyExistsAsync_Should_Return_False_For_NonExisting()
        {
            await ClearTestCompaniesAsync();

            var exists = await CreateQueryRepo().CompanyExistsAsync("DefinitelyNonExistentCo_XYZ");

            exists.Should().BeFalse();
        }
    }
}
