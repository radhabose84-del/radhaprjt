using AutoMapper;
using Contracts.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
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
    public sealed class CompanyCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CompanyCommandRepositoryTests(DbFixture fixture)
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

        private CompanyCommandRepository CreateRepository(ApplicationDbContext ctx)
        {
            var mapperMock = new Mock<IMapper>(MockBehavior.Loose);
            var httpContextMock = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
            return new CompanyCommandRepository(ctx, mapperMock.Object, httpContextMock.Object);
        }

        private static Company BuildCompany(
            string name = "Test Company",
            string legalName = "Test Company Pvt Ltd",
            string gst = "GSTTEST1234",
            string pan = "PAN1234")
        {
            return new Company
            {
                CompanyName = name,
                LegalName = legalName,
                GstNumber = gst,
                TIN = "TIN1234",
                TAN = "TAN1234",
                CSTNo = "CST1234",
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
                    Name = "Contact Person",
                    Designation = "Manager",
                    Email = "contact@example.com",
                    Phone = "9999999999",
                    Remarks = "Primary contact"
                }
            };
        }

        /// <summary>
        /// Soft-delete any existing test companies to keep tests isolated without
        /// breaking FK constraints from other integration tests.
        /// </summary>
        private async Task ClearTestCompaniesAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThan_Zero()
        {
            await using var ctx = CreateDbContext();
            await ClearTestCompaniesAsync(ctx);

            var repo = CreateRepository(ctx);
            var company = BuildCompany("Test Company A", "Test Company A Pvt", "GSTA1", "PANA1");

            var newId = await repo.CreateAsync(company);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTestCompaniesAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(
                BuildCompany("Test Company B", "Test Company B Pvt", "GSTB2", "PANB2"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Companies.FirstOrDefaultAsync(c => c.Id == newId);

            saved.Should().NotBeNull();
            saved!.CompanyName.Should().Be("Test Company B");
            saved.LegalName.Should().Be("Test Company B Pvt");
            saved.GstNumber.Should().Be("GSTB2");
            saved.PanNumber.Should().Be("PANB2");
            saved.EntityId.Should().Be(1);
            saved.IsActive.Should().Be(Enums.Status.Active);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = CreateDbContext();
            await ClearTestCompaniesAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(
                BuildCompany("Test Company C", "Test Company C Pvt", "GSTC3", "PANC3"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Companies.FirstOrDefaultAsync(c => c.Id == newId);

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
            await ClearTestCompaniesAsync(ctx);

            var repo = CreateRepository(ctx);
            var created = BuildCompany("Update Co Orig", "Update Co Orig Pvt", "GSTUPD1", "PANUPD1");
            var id = await repo.CreateAsync(created);
            ctx.ChangeTracker.Clear();

            var updated = BuildCompany("Update Co New", "Update Co New Pvt", "GSTUPD2", "PANUPD2");
            var result = await repo.UpdateAsync(id, updated);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var saved = await ctx.Companies.FirstOrDefaultAsync(c => c.Id == id);

            saved.Should().NotBeNull();
            saved!.CompanyName.Should().Be("Update Co New");
            saved.LegalName.Should().Be("Update Co New Pvt");
            saved.PanNumber.Should().Be("PANUPD2");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var result = await repo.UpdateAsync(999999, BuildCompany("NotFoundCo", "NotFoundCo", "GSTX", "PANX"));

            result.Should().BeFalse();
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = CreateDbContext();
            await ClearTestCompaniesAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(
                BuildCompany("Delete Co One", "Delete Co One Pvt", "GSTD1", "PAND1"));
            ctx.ChangeTracker.Clear();

            var deleteModel = new Company { IsDeleted = Enums.IsDelete.Deleted };
            var result = await repo.DeleteAsync(id, deleteModel);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var saved = await ctx.Companies.FirstOrDefaultAsync(c => c.Id == id);

            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();
            var repo = CreateRepository(ctx);

            var deleteModel = new Company { IsDeleted = Enums.IsDelete.Deleted };
            var result = await repo.DeleteAsync(999999, deleteModel);

            result.Should().BeFalse();
        }
    }
}
