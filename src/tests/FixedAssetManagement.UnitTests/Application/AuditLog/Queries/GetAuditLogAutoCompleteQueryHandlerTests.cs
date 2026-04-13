using Contracts.Common;
using FAM.Application.AuditLog.Queries;
using FAM.Application.AuditLog.Queries.GetAuditLog;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AuditLog.Queries
{
    /// <summary>
    /// The file GetAuditLogAutoCompleteQueryHandler.cs in the FAM module physically declares
    /// the class GetAuditLogBySearchPatternQueryHandler and uses IMongoDatabase directly
    /// (not a repository interface). Due to MongoDB.Driver version conflicts
    /// (v2 IAsyncCursor vs v3) direct handler mocking is not feasible in unit tests.
    /// These tests verify query contract surface (type, default values, nullable assignment).
    /// </summary>
    public sealed class GetAuditLogAutoCompleteQueryHandlerTests
    {
        [Fact]
        public void Query_ShouldImplement_IRequest_With_ApiResponseDTO_List_AuditLogDto()
        {
            var query = new GetAuditLogBySearchPatternQuery();
            query.Should().BeAssignableTo<IRequest<ApiResponseDTO<List<AuditLogDto>>>>();
        }

        [Fact]
        public void Query_DefaultSearchPattern_ShouldBeNull()
        {
            var query = new GetAuditLogBySearchPatternQuery();
            query.SearchPattern.Should().BeNull();
        }

        [Fact]
        public void Query_SearchPattern_ShouldSupportLongValue()
        {
            var longPattern = new string('A', 256);
            var query = new GetAuditLogBySearchPatternQuery { SearchPattern = longPattern };
            query.SearchPattern.Should().Be(longPattern);
        }

        [Fact]
        public void Query_SearchPattern_ShouldSupportEmptyString()
        {
            var query = new GetAuditLogBySearchPatternQuery { SearchPattern = string.Empty };
            query.SearchPattern.Should().Be(string.Empty);
        }

        [Fact]
        public void AuditLogDto_ShouldBeConstructable_WithAllFields()
        {
            var now = DateTimeOffset.UtcNow;
            var dto = new AuditLogDto
            {
                Id = "abc123",
                MachineName = "test-machine",
                IPAddress = "127.0.0.1",
                OS = "Windows",
                Browser = "Chrome",
                Action = "Create",
                Details = "Asset created",
                Module = "AssetMaster",
                CreatedAt = now,
                CreatedBy = 1,
                CreatedByName = "admin"
            };

            dto.Id.Should().Be("abc123");
            dto.Action.Should().Be("Create");
            dto.Module.Should().Be("AssetMaster");
            dto.CreatedBy.Should().Be(1);
            dto.CreatedAt.Should().Be(now);
        }
    }
}
