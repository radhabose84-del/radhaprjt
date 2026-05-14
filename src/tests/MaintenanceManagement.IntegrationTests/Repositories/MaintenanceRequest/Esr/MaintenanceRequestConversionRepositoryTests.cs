using MaintenanceManagement.Infrastructure.Repositories.Operations;
using Microsoft.Data.SqlClient;

namespace MaintenanceManagement.IntegrationTests.Repositories.MaintenanceRequest.Esr
{
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceRequestConversionRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceRequestConversionRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceRequestConversionRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<EsrTestSeedHelper> SetupAsync()
        {
            await _fixture.ClearAllTablesAsync();
            var seed = new EsrTestSeedHelper(_fixture);
            await seed.SeedStatusesAndTypesAsync();
            return seed;
        }

        // --- Status transitions ---

        [Fact]
        public async Task PartialConversion_Sets_PartiallyConverted_Status()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusOpenId,
                estimatedServiceCost: 100000m, convertedToPoAmount: 0m);

            var applied = await CreateRepo().ApplyServicePoConversionAsync(id, 50000m);

            applied.Should().BeTrue();
            var (amount, status) = await seed.GetRequestStateAsync(id);
            amount.Should().Be(50000);
            status.Should().Be(seed.StatusPartiallyConvertedId);
        }

        [Fact]
        public async Task FullConversion_Sets_FullyConverted_Status()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusOpenId,
                estimatedServiceCost: 100000m, convertedToPoAmount: 0m);

            var applied = await CreateRepo().ApplyServicePoConversionAsync(id, 100000m);

            applied.Should().BeTrue();
            var (amount, status) = await seed.GetRequestStateAsync(id);
            amount.Should().Be(100000);
            status.Should().Be(seed.StatusFullyConvertedId);
        }

        [Fact]
        public async Task ExceedingEstimate_Also_Sets_FullyConverted()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusOpenId,
                estimatedServiceCost: 100000m, convertedToPoAmount: 0m);

            var applied = await CreateRepo().ApplyServicePoConversionAsync(id, 150000m);

            applied.Should().BeTrue();
            var (amount, status) = await seed.GetRequestStateAsync(id);
            amount.Should().Be(150000);
            status.Should().Be(seed.StatusFullyConvertedId);
        }

        [Fact]
        public async Task AddingToPartiallyConverted_AdvancesToFullyConverted_When_Reaching_Estimate()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusPartiallyConvertedId,
                estimatedServiceCost: 100000m, convertedToPoAmount: 60000m);

            var applied = await CreateRepo().ApplyServicePoConversionAsync(id, 40000m);

            applied.Should().BeTrue();
            var (amount, status) = await seed.GetRequestStateAsync(id);
            amount.Should().Be(100000);
            status.Should().Be(seed.StatusFullyConvertedId);
        }

        [Fact]
        public async Task NegativeDelta_ClampsTo_Zero_And_Reverts_To_Open()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusPartiallyConvertedId,
                estimatedServiceCost: 100000m, convertedToPoAmount: 30000m);

            // PO was cancelled / unlinked — subtract its 30000 contribution
            var applied = await CreateRepo().ApplyServicePoConversionAsync(id, -30000m);

            applied.Should().BeTrue();
            var (amount, status) = await seed.GetRequestStateAsync(id);
            amount.Should().Be(0);
            status.Should().Be(seed.StatusOpenId);
        }

        [Fact]
        public async Task NegativeDelta_Beyond_Current_Clamps_To_Zero()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusPartiallyConvertedId,
                estimatedServiceCost: 100000m, convertedToPoAmount: 30000m);

            // Subtract more than current — should clamp to 0
            var applied = await CreateRepo().ApplyServicePoConversionAsync(id, -50000m);

            applied.Should().BeTrue();
            var (amount, status) = await seed.GetRequestStateAsync(id);
            amount.Should().Be(0);
            status.Should().Be(seed.StatusOpenId);
        }

        [Fact]
        public async Task NonExistent_Request_Returns_False()
        {
            await SetupAsync();

            var applied = await CreateRepo().ApplyServicePoConversionAsync(999999, 50000m);

            applied.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleted_Request_Returns_False()
        {
            var seed = await SetupAsync();
            var id = await seed.SeedMaintenanceRequestAsync(
                seed.RequestTypeExternalId, seed.StatusOpenId, isDeleted: true);

            var applied = await CreateRepo().ApplyServicePoConversionAsync(id, 50000m);

            applied.Should().BeFalse();
        }
    }
}
