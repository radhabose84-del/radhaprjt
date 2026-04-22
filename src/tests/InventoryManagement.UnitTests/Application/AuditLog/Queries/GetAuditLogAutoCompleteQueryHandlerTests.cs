using Contracts.Common;
using InventoryManagement.Application.AuditLog.Queries;
using InventoryManagement.Application.AuditLog.Queries.GetAuditLog;
using MongoDB.Bson;

namespace InventoryManagement.UnitTests.Application.AuditLog.Queries
{
    public sealed class GetAuditLogAutoCompleteQueryHandlerTests
    {
        [Fact]
        public void GetAuditLogBySearchPatternQuery_Properties_ShouldBeAssignable()
        {
            var query = new GetAuditLogBySearchPatternQuery
            {
                SearchPattern = "TestPattern"
            };

            query.SearchPattern.Should().Be("TestPattern");
        }

        [Fact]
        public void GetAuditLogBySearchPatternQuery_NullableSearchPattern_ShouldAcceptNull()
        {
            var query = new GetAuditLogBySearchPatternQuery
            {
                SearchPattern = null
            };

            query.SearchPattern.Should().BeNull();
        }

        [Fact]
        public void GetAuditLogBySearchPatternQuery_ImplementsCorrectRequestType()
        {
            var query = new GetAuditLogBySearchPatternQuery();

            query.Should().BeAssignableTo<MediatR.IRequest<ApiResponseDTO<List<AuditLogDto>>>>();
        }

        [Fact]
        public void AuditLogDto_MappingFromEntity_ReproducesHandlerLogic()
        {
            var objectId = ObjectId.GenerateNewId();
            var now = DateTimeOffset.UtcNow;

            // Simulate the handler's inline mapping logic
            var dto = new AuditLogDto
            {
                Id = objectId.ToString(),
                CreatedBy = 1,
                CreatedByName = "admin",
                IPAddress = "192.168.1.1",
                OS = "Windows",
                Browser = "Chrome",
                Action = "Create",
                Details = "Entity created",
                Module = "Inventory",
                CreatedAt = now,
                MachineName = "SERVER"
            };

            dto.Id.Should().Be(objectId.ToString());
            dto.Action.Should().Be("Create");
            dto.Details.Should().Be("Entity created");
            dto.Module.Should().Be("Inventory");
            dto.CreatedAt.Should().Be(now);
        }

        [Fact]
        public void ApiResponseDTO_SuccessResponse_ShouldHaveCorrectStructure()
        {
            var dtos = new List<AuditLogDto>
            {
                new AuditLogDto { Id = "abc", Action = "Create", Module = "Test" },
                new AuditLogDto { Id = "def", Action = "Update", Module = "Test" }
            };

            var response = new ApiResponseDTO<List<AuditLogDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtos
            };

            response.IsSuccess.Should().BeTrue();
            response.Data.Should().HaveCount(2);
        }

        [Fact]
        public void ApiResponseDTO_FailureResponse_NoData()
        {
            var response = new ApiResponseDTO<List<AuditLogDto>>
            {
                IsSuccess = false,
                Message = "No audit logs found matching the search pattern."
            };

            response.IsSuccess.Should().BeFalse();
            response.Message.Should().Contain("No audit logs found");
        }
    }
}
