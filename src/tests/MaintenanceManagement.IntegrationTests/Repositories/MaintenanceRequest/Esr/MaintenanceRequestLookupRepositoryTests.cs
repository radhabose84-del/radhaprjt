using MaintenanceManagement.Infrastructure.Repositories.Lookups.Maintenance;
using Microsoft.Data.SqlClient;

namespace MaintenanceManagement.IntegrationTests.Repositories.MaintenanceRequest.Esr
{
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceRequestLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceRequestLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceRequestLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<EsrTestSeedHelper> SetupAsync()
        {
            await _fixture.ClearAllTablesAsync();
            var seed = new EsrTestSeedHelper(_fixture);
            await seed.SeedStatusesAndTypesAsync();
            return seed;
        }

        [Fact]
        public async Task GetAvailableForServicePoAsync_Includes_Open_External()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                requestTypeId: seed.RequestTypeExternalId,
                statusId: seed.StatusOpenId);

            var rows = await CreateRepo().GetAvailableForServicePoAsync(null, CancellationToken.None);

            rows.Should().ContainSingle(r => r.Id == id);
        }

        [Fact]
        public async Task GetAvailableForServicePoAsync_Includes_InProgress_External()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                requestTypeId: seed.RequestTypeExternalId,
                statusId: seed.StatusInProgressId);

            var rows = await CreateRepo().GetAvailableForServicePoAsync(null, CancellationToken.None);

            rows.Should().ContainSingle(r => r.Id == id);
        }

        [Fact]
        public async Task GetAvailableForServicePoAsync_Includes_PartiallyConverted_External()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                requestTypeId: seed.RequestTypeExternalId,
                statusId: seed.StatusPartiallyConvertedId);

            var rows = await CreateRepo().GetAvailableForServicePoAsync(null, CancellationToken.None);

            rows.Should().ContainSingle(r => r.Id == id);
        }

        [Fact]
        public async Task GetAvailableForServicePoAsync_Excludes_Closed_External()
        {
            var seed = await SetupAsync();
            await seed.SeedMaintenanceRequestAsync(
                requestTypeId: seed.RequestTypeExternalId,
                statusId: seed.StatusClosedId);

            var rows = await CreateRepo().GetAvailableForServicePoAsync(null, CancellationToken.None);

            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAvailableForServicePoAsync_Excludes_FullyConverted_External()
        {
            var seed = await SetupAsync();
            await seed.SeedMaintenanceRequestAsync(
                requestTypeId: seed.RequestTypeExternalId,
                statusId: seed.StatusFullyConvertedId);

            var rows = await CreateRepo().GetAvailableForServicePoAsync(null, CancellationToken.None);

            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAvailableForServicePoAsync_Excludes_Internal_Requests()
        {
            var seed = await SetupAsync();
            await seed.SeedMaintenanceRequestAsync(
                requestTypeId: seed.RequestTypeInternalId,
                statusId: seed.StatusOpenId);

            var rows = await CreateRepo().GetAvailableForServicePoAsync(null, CancellationToken.None);

            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAvailableForServicePoAsync_Excludes_Inactive()
        {
            var seed = await SetupAsync();
            await seed.SeedMaintenanceRequestAsync(
                requestTypeId: seed.RequestTypeExternalId,
                statusId: seed.StatusOpenId,
                isActive: false);

            var rows = await CreateRepo().GetAvailableForServicePoAsync(null, CancellationToken.None);

            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAvailableForServicePoAsync_Excludes_SoftDeleted()
        {
            var seed = await SetupAsync();
            await seed.SeedMaintenanceRequestAsync(
                requestTypeId: seed.RequestTypeExternalId,
                statusId: seed.StatusOpenId,
                isDeleted: true);

            var rows = await CreateRepo().GetAvailableForServicePoAsync(null, CancellationToken.None);

            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Null_When_Not_Found()
        {
            await SetupAsync();

            var result = await CreateRepo().GetByIdAsync(999999, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Even_For_Closed_Requests()
        {
            // GetByIdAsync is intentionally less restrictive than the dropdown filter —
            // it surfaces the row regardless of status for display purposes.
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                requestTypeId: seed.RequestTypeExternalId,
                statusId: seed.StatusClosedId);

            var result = await CreateRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.RequestStatusCode.Should().Be("Closed");
        }

        [Fact]
        public async Task GetByIdsAsync_Returns_Empty_For_Empty_Input()
        {
            await SetupAsync();

            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>(), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
