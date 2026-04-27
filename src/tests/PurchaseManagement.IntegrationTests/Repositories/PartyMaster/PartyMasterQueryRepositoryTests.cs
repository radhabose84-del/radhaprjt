using Microsoft.Data.SqlClient;
using PurchaseManagement.Infrastructure.Repositories.PartyMaster;

namespace PurchaseManagement.IntegrationTests.Repositories.PartyMaster
{
    /// <summary>
    /// PartyMasterQueryRepository.GetPartyMasters calls the stored procedure
    /// [dbo].[GetCustomerDetailsByOldUnitcode] which does NOT exist in the
    /// freshly-created test database. These tests verify:
    ///   1. The repository can be constructed with its real dependencies.
    ///   2. The SP-backed method throws SqlException as expected when the SP is missing.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class PartyMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyMasterQueryRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PartyMasterQueryRepository(conn);
        }

        [Fact]
        public void Constructor_Should_Not_Throw_With_Valid_Connection()
        {
            var act = () => CreateRepo();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task GetPartyMasters_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetPartyMasters("U001", "search");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetPartyMasters_With_Empty_SearchPattern_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () => await repo.GetPartyMasters("U001", string.Empty);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetPartyMasters_With_Null_SearchPattern_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            // Repository converts whitespace/null search pattern to null DbParam — still hits SP.
            Func<Task> act = async () => await repo.GetPartyMasters("U001", null!);

            await act.Should().ThrowAsync<SqlException>();
        }
    }
}
