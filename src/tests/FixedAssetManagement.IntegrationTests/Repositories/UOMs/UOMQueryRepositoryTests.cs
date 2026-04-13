using Contracts.Interfaces.Validations.MaintenanceManagement;
using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;
using FAM.Infrastructure.Repositories.UOMs;

namespace FixedAssetManagement.IntegrationTests.Repositories.UOMs
{
    [Collection("DatabaseCollection")]
    public sealed class UOMQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UOMQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UOMQueryRepository CreateQueryRepo(bool linked = false, bool active = false)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var validation = new Mock<IMaintenanceUomValidation>(MockBehavior.Loose);
            validation.Setup(v => v.HasLinkedUomAsync(It.IsAny<int>())).ReturnsAsync(linked);
            validation.Setup(v => v.HasActiveUomAsync(It.IsAny<int>())).ReturnsAsync(active);
            return new UOMQueryRepository(conn, validation.Object);
        }

        private async Task<int> SeedMiscMasterAsync(string typeCode)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var typeId = (await new MiscTypeMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = typeCode,
                Description = "Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;

            return (await new MiscMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId,
                Code = "MM_" + typeCode,
                Description = "Misc",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
        }

        private async Task<int> SeedEntityAsync(int typeId, string code = "UOMQ_001", string name = "UOM Q")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new UOMCommandRepository(ctx).CreateAsync(new UOM
            {
                Code = code,
                UOMName = name,
                UOMTypeId = typeId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllUOMAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var typeId = await SeedMiscMasterAsync("UOMQ_T1");
            await SeedEntityAsync(typeId);

            var (items, total) = await CreateQueryRepo().GetAllUOMAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllUOMAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var typeId = await SeedMiscMasterAsync("UOMQ_T2");
            await SeedEntityAsync(typeId, "UOMQ_A", "Alpha");
            await SeedEntityAsync(typeId, "UOMQ_B", "Beta");

            var (items, _) = await CreateQueryRepo().GetAllUOMAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].UOMName.Should().Be("Alpha");
        }

        [Fact]
        public async Task GetAllUOMAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var typeId = await SeedMiscMasterAsync("UOMQ_T3");
            var id = await SeedEntityAsync(typeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new UOMCommandRepository(ctx).DeleteAsync(id, new UOM { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllUOMAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var typeId = await SeedMiscMasterAsync("UOMQ_T4");
            var id = await SeedEntityAsync(typeId, "UOMQ_ID", "ById");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Code.Should().Be("UOMQ_ID");
        }

        [Fact]
        public async Task GetByUOMNameAsync_Should_Return_Match()
        {
            await ClearTablesAsync();
            var typeId = await SeedMiscMasterAsync("UOMQ_T5");
            await SeedEntityAsync(typeId, "UOMQ_N", "ByName");

            var result = await CreateQueryRepo().GetByUOMNameAsync("ByName");

            result.Should().NotBeNull();
            result!.Code.Should().Be("UOMQ_N");
        }

        [Fact]
        public async Task GetByUOMNameAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await ClearTablesAsync();
            var typeId = await SeedMiscMasterAsync("UOMQ_T6");
            var id = await SeedEntityAsync(typeId, "UOMQ_N2", "ExcludeMe");

            var result = await CreateQueryRepo().GetByUOMNameAsync("ExcludeMe", id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUOM_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var typeId = await SeedMiscMasterAsync("UOMQ_T7");
            await SeedEntityAsync(typeId, "UOMQ_AC", "AutoUom");

            var result = await CreateQueryRepo().GetUOM("Auto");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task IsUomLinkedAsync_Should_Return_False_When_NotLinked()
        {
            await ClearTablesAsync();

            (await CreateQueryRepo(active: false).IsUomLinkedAsync(1)).Should().BeFalse();
        }

        [Fact]
        public async Task IsUomLinkedAsync_Should_Return_True_When_Active_Cross_Module_Link()
        {
            await ClearTablesAsync();

            (await CreateQueryRepo(active: true).IsUomLinkedAsync(1)).Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTablesAsync();
            var typeId = await SeedMiscMasterAsync("UOMQ_T8");
            var id = await SeedEntityAsync(typeId);

            (await CreateQueryRepo(linked: false).SoftDeleteValidationAsync(id)).Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_Cross_Module_Linked()
        {
            await ClearTablesAsync();
            var typeId = await SeedMiscMasterAsync("UOMQ_T9");
            var id = await SeedEntityAsync(typeId);

            (await CreateQueryRepo(linked: true).SoftDeleteValidationAsync(id)).Should().BeTrue();
        }
    }
}
