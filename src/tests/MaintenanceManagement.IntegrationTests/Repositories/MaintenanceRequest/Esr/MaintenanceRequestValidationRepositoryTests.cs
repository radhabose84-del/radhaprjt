using MaintenanceManagement.Infrastructure.Repositories.Validations;
using Microsoft.Data.SqlClient;

namespace MaintenanceManagement.IntegrationTests.Repositories.MaintenanceRequest.Esr
{
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceRequestValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceRequestValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceRequestValidationRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<EsrTestSeedHelper> SetupAsync()
        {
            await _fixture.ClearAllTablesAsync();
            var seed = new EsrTestSeedHelper(_fixture);
            await seed.SeedStatusesAndTypesAsync();
            return seed;
        }

        // --- IsAvailableForServicePoAsync ---

        [Fact]
        public async Task IsAvailable_True_For_Open_External()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusOpenId);

            var result = await CreateRepo().IsAvailableForServicePoAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsAvailable_True_For_PartiallyConverted_External()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusPartiallyConvertedId);

            var result = await CreateRepo().IsAvailableForServicePoAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsAvailable_False_For_FullyConverted_External()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusFullyConvertedId);

            var result = await CreateRepo().IsAvailableForServicePoAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsAvailable_False_For_Closed_External()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusClosedId);

            var result = await CreateRepo().IsAvailableForServicePoAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsAvailable_False_For_Internal_Request_Even_If_Open()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeInternalId, seed.StatusOpenId);

            var result = await CreateRepo().IsAvailableForServicePoAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsAvailable_False_For_SoftDeleted()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusOpenId, isDeleted: true);

            var result = await CreateRepo().IsAvailableForServicePoAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsAvailable_False_For_NonExistent_Request()
        {
            await SetupAsync();

            var result = await CreateRepo().IsAvailableForServicePoAsync(999999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // --- GetRefAsync ---

        [Fact]
        public async Task GetRefAsync_Returns_Vendor_And_ServiceType()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusOpenId,
                vendorId: 42, serviceTypeId: 99);

            var result = await CreateRepo().GetRefAsync(id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.VendorId.Should().Be(42);
            result.ServiceTypeId.Should().Be(99);
            result.RequestStatusCode.Should().Be("Open");
        }

        [Fact]
        public async Task GetRefAsync_Returns_Null_For_Missing_Request()
        {
            await SetupAsync();

            var result = await CreateRepo().GetRefAsync(999999, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
