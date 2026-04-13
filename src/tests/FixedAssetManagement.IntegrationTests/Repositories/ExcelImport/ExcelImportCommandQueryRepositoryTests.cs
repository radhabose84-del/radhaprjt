using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FAM.Infrastructure.Repositories.ExcelImport;

namespace FixedAssetManagement.IntegrationTests.Repositories.ExcelImport
{
    public sealed class ExcelImportCommandQueryRepositoryTests
    {
        private static ExcelImportCommandQueryRepository CreateRepo(
            List<DepartmentLookupDto> depts = null,
            List<UnitLookupDto> units = null,
            List<CompanyLookupDto> companies = null)
        {
            var deptMock = new Mock<IDepartmentLookup>(MockBehavior.Loose);
            deptMock.Setup(x => x.GetAllDepartmentAsync()).ReturnsAsync(depts ?? new List<DepartmentLookupDto>());

            var unitMock = new Mock<IUnitLookup>(MockBehavior.Loose);
            unitMock.Setup(x => x.GetAllUnitAsync()).ReturnsAsync(units ?? new List<UnitLookupDto>());

            var companyMock = new Mock<ICompanyLookup>(MockBehavior.Loose);
            companyMock.Setup(x => x.GetAllCompanyAsync()).ReturnsAsync(companies ?? new List<CompanyLookupDto>());

            return new ExcelImportCommandQueryRepository(unitMock.Object, deptMock.Object, companyMock.Object);
        }

        [Fact]
        public async Task GetAssetDeptIdByNameAsync_Should_Return_Match()
        {
            var repo = CreateRepo(depts: new List<DepartmentLookupDto>
            {
                new() { DepartmentId = 5, DepartmentName = "Production" }
            });

            var result = await repo.GetAssetDeptIdByNameAsync("prod");

            result.Should().Be(5);
        }

        [Fact]
        public async Task GetAssetDeptIdByNameAsync_Should_Return_Null_When_NotFound()
        {
            var repo = CreateRepo(depts: new List<DepartmentLookupDto>
            {
                new() { DepartmentId = 5, DepartmentName = "Production" }
            });

            var result = await repo.GetAssetDeptIdByNameAsync("Sales");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAssetUnitIdByNameAsync_Should_Return_Match()
        {
            var repo = CreateRepo(units: new List<UnitLookupDto>
            {
                new() { UnitId = 9, UnitName = "MainPlant", ShortName = "MP", UnitHeadName = "X", OldUnitId = "OLD" }
            });

            var result = await repo.GetAssetUnitIdByNameAsync("Main");

            result.Should().Be(9);
        }

        [Fact]
        public async Task GetAssetUnitIdByNameAsync_Should_Return_Null_When_NotFound()
        {
            var repo = CreateRepo(units: new List<UnitLookupDto>());

            var result = await repo.GetAssetUnitIdByNameAsync("Anything");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCompanyByNameAsync_Should_Return_CompanyName()
        {
            var repo = CreateRepo(companies: new List<CompanyLookupDto>
            {
                new() { CompanyId = 1, CompanyName = "Acme Corp" }
            });

            var result = await repo.GetCompanyByNameAsync(1);

            result.Should().Be("Acme Corp");
        }

        [Fact]
        public async Task GetCompanyByNameAsync_Should_Return_Null_When_NotFound()
        {
            var repo = CreateRepo(companies: new List<CompanyLookupDto>());

            var result = await repo.GetCompanyByNameAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUnitByNameAsync_Should_Return_UnitDto_When_Found()
        {
            var repo = CreateRepo(units: new List<UnitLookupDto>
            {
                new() { UnitId = 7, UnitName = "U7", ShortName = "U7S", UnitHeadName = "Head", OldUnitId = "OLD7" }
            });

            var result = await repo.GetUnitByNameAsync(7);

            result.Should().NotBeNull();
            result!.ShortName.Should().Be("U7S");
            result.OldUnitId.Should().Be("OLD7");
        }

        [Fact]
        public async Task GetUnitByNameAsync_Should_Return_Null_When_NotFound()
        {
            var repo = CreateRepo(units: new List<UnitLookupDto>());

            var result = await repo.GetUnitByNameAsync(999);

            result.Should().BeNull();
        }
    }
}
