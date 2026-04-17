using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;
using UserManagement.Infrastructure.Repositories.Profile;
using UserManagement.IntegrationTests.Common;

namespace UserManagement.IntegrationTests.Repositories.Profile
{
    /// <summary>
    /// Integration tests for ProfileQueryRepository.
    /// GetUnit(userId) joins AppData.Unit + AppSecurity.UserUnit + AppData.Division + AppData.Company.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ProfileQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProfileQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ProfileQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        [Fact]
        public async Task GetUnit_Should_Return_Empty_When_UserHasNoUnits()
        {
            var result = await CreateRepo().GetUnit(999999);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
