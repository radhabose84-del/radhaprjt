using FluentAssertions;
using Microsoft.Data.SqlClient;
using UserManagement.Infrastructure.Repositories.UserRoleAllocation.UserRoleAllocationQueryRepository;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.UserRoleAllocation
{
    /// <summary>
    /// UserRoleAllocationQueryRepository currently has SQL that references
    /// UserRole.UserRoleId as a join column. UserRole's actual PK column is 'Id',
    /// so every query will throw a SqlException at runtime.
    ///
    /// These tests document the existing (broken) behaviour so that when the SQL
    /// is corrected, these assertions will fail and prompt a refresh.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class UserRoleAllocationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UserRoleAllocationQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UserRoleAllocationQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new UserRoleAllocationQueryRepository(conn);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var repo = CreateQueryRepo();
            repo.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllAsync_Should_Throw_SqlException_Due_To_Broken_Join_Column()
        {
            var repo = CreateQueryRepo();

            Func<Task> act = async () => await repo.GetAllAsync();

            // The join references ur.UserRoleId which does not exist on UserRole
            // (PK is 'Id'). This currently throws a SqlException at runtime.
            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_SqlException_Due_To_Broken_Join_Column()
        {
            var repo = CreateQueryRepo();

            Func<Task> act = async () => await repo.GetByIdAsync(1);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetByUserIdAsync_Should_Throw_SqlException_Due_To_Broken_Join_Column()
        {
            var repo = CreateQueryRepo();

            Func<Task> act = async () => await repo.GetByUserIdAsync(1);

            await act.Should().ThrowAsync<SqlException>();
        }
    }
}
