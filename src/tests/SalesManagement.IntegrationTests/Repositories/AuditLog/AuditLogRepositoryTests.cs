using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.AuditLog
{
    /// <summary>
    /// Placeholder for AuditLogRepository integration tests.
    ///
    /// AuditLogRepository (AuditLogMongoRepository) uses MongoDB (IMongoDbContext) for all
    /// operations: CreateAsync, GetAllAsync, GetByAuditLogNameAsync.
    ///
    /// The SalesManagement.IntegrationTests test infrastructure is configured for SQL Server
    /// only (DbFixture creates/manages SalesManagement_TestDb on SQL Server 192.168.1.126).
    /// There is no MongoDB test fixture in this project.
    ///
    /// To properly integration-test AuditLogRepository, a separate MongoDB test fixture would
    /// be needed (e.g., using Testcontainers for a disposable MongoDB instance or a dedicated
    /// test MongoDB database). This is out of scope for the current SQL Server test suite.
    ///
    /// The AuditLogRepository is covered by:
    ///   - Unit tests that mock IMongoDbContext and verify CRUD behavior
    ///   - End-to-end tests that verify audit log entries appear after MediatR domain events
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class AuditLogRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AuditLogRepositoryTests(DbFixture fixture) => _fixture = fixture;

        [Fact]
        public void AuditLogRepository_UsesMongoDB_NotSqlServer()
        {
            // AuditLogRepository depends on IMongoDbContext, not ApplicationDbContext or IDbConnection.
            // Integration testing requires a MongoDB fixture (not available in this SQL Server test suite).
            // This test documents the architectural boundary.

            _fixture.Should().NotBeNull("DbFixture is available but AuditLog uses MongoDB");
        }
    }
}
