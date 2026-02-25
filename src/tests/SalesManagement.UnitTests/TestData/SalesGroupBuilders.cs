#nullable disable
using SalesManagement.Application.SalesGroup.Commands.CreateSalesGroup;
using SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup;
using SalesManagement.Application.SalesGroup.Dto;

namespace SalesManagement.UnitTests.TestData
{
    /// <summary>
    /// Builder helpers for creating valid test data for SalesGroup tests.
    /// </summary>
    public static class SalesGroupBuilders
    {
        // ── Create Command ────────────────────────────────────────────────────

        public static CreateSalesGroupCommand ValidCreateCommand(
            string name = "Test Sales Group",
            int salesOfficeId = 1,
            string responsibleManager = "John Doe",
            int? productCategoryId = null,
            string regionTerritory = "North") =>
            new CreateSalesGroupCommand
            {
                SalesGroupName = name,
                SalesOfficeId = salesOfficeId,
                ResponsibleManager = responsibleManager,
                ProductCategoryId = productCategoryId,
                RegionTerritory = regionTerritory
            };

        // ── Update Command ────────────────────────────────────────────────────

        public static UpdateSalesGroupCommand ValidUpdateCommand(
            int id = 1,
            string name = "Updated Sales Group",
            int salesOfficeId = 1,
            string responsibleManager = "Jane Smith",
            int? productCategoryId = null,
            string regionTerritory = "South",
            int isActive = 1) =>
            new UpdateSalesGroupCommand
            {
                Id = id,
                SalesGroupName = name,
                SalesOfficeId = salesOfficeId,
                ResponsibleManager = responsibleManager,
                ProductCategoryId = productCategoryId,
                RegionTerritory = regionTerritory,
                IsActive = isActive
            };

        // ── Full DTO ──────────────────────────────────────────────────────────

        public static SalesGroupDto ValidDto(
            int id = 1,
            string name = "Test Sales Group",
            int salesOfficeId = 1,
            string salesOfficeName = "Test Sales Office",
            string responsibleManager = "John Doe",
            int? productCategoryId = null,
            string productCategoryName = null,
            string regionTerritory = "North") =>
            new SalesGroupDto
            {
                Id = id,
                SalesGroupName = name,
                SalesOfficeId = salesOfficeId,
                SalesOfficeName = salesOfficeName,
                ResponsibleManager = responsibleManager,
                ProductCategoryId = productCategoryId,
                ProductCategoryName = productCategoryName,
                RegionTerritory = regionTerritory,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        // ── Lookup DTO List ───────────────────────────────────────────────────

        public static IReadOnlyList<SalesGroupLookupDto> ValidLookupList() =>
            new List<SalesGroupLookupDto>
            {
                new SalesGroupLookupDto { Id = 1, SalesGroupName = "Group One", SalesOfficeId = 1 },
                new SalesGroupLookupDto { Id = 2, SalesGroupName = "Group Two", SalesOfficeId = 2 }
            };

        // ── Domain Entity ─────────────────────────────────────────────────────

        public static SalesManagement.Domain.Entities.SalesGroup ValidEntity(int id = 1) =>
            new SalesManagement.Domain.Entities.SalesGroup
            {
                Id = id,
                SalesGroupName = "Test Sales Group",
                SalesOfficeId = 1,
                ResponsibleManager = "John Doe",
                ProductCategoryId = null,
                RegionTerritory = "North",
                IsActive = SalesManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = SalesManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
