using FluentAssertions;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Profile
{
    /// <summary>
    /// ProfileCommandRepository is an empty class (implements IProfileCommand with zero methods).
    /// This test verifies instantiation only.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ProfileCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProfileCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        [Fact]
        public void Repository_ShouldBeInstantiable()
        {
            var repo = new UserManagement.Infrastructure.Repositories.Profile.ProfileCommandRepository();
            repo.Should().NotBeNull();
        }
    }
}
