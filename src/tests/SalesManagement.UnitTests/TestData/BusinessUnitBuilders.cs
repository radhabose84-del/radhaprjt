using SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit;
using SalesManagement.Application.BusinessUnit.Commands.UpdateBusinessUnit;
using SalesManagement.Application.BusinessUnit.Dto;

namespace SalesManagement.UnitTests.TestData
{
    /// <summary>
    /// Builder helpers for creating valid test data for BusinessUnit tests.
    /// BusinessUnit has no AutoMapper — entity is built manually with new().
    /// BusinessUnit has Description (optional, max 500).
    /// </summary>
    public static class BusinessUnitBuilders
    {
        // ── Create Command ────────────────────────────────────────────────────

        public static CreateBusinessUnitCommand ValidCreateCommand(
            string? code = "BU001",
            string? name = "Test Business Unit",
            string? description = "Test Description") =>
            new CreateBusinessUnitCommand
            {
                BusinessUnitCode = code!,
                BusinessUnitName = name!,
                Description = description!
            };

        // ── Update Command ────────────────────────────────────────────────────

        public static UpdateBusinessUnitCommand ValidUpdateCommand(
            int id = 1,
            string? name = "Updated Business Unit",
            string? description = "Updated Description",
            int isActive = 1) =>
            new UpdateBusinessUnitCommand
            {
                Id = id,
                BusinessUnitName = name!,
                Description = description!,
                IsActive = isActive
            };

        // ── Full DTO ──────────────────────────────────────────────────────────

        public static BusinessUnitDto ValidDto(
            int id = 1,
            string code = "BU001",
            string name = "Test Business Unit",
            string description = "Test Description") =>
            new BusinessUnitDto
            {
                Id = id,
                BusinessUnitCode = code,
                BusinessUnitName = name,
                Description = description,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        // ── Lookup DTO List ───────────────────────────────────────────────────

        public static IReadOnlyList<BusinessUnitLookupDto> ValidLookupList() =>
            new List<BusinessUnitLookupDto>
            {
                new BusinessUnitLookupDto { Id = 1, BusinessUnitCode = "BU001", BusinessUnitName = "Business Unit One" },
                new BusinessUnitLookupDto { Id = 2, BusinessUnitCode = "BU002", BusinessUnitName = "Business Unit Two" }
            };

        // ── Domain Entity ─────────────────────────────────────────────────────

        public static SalesManagement.Domain.Entities.BusinessUnit ValidEntity(int id = 1) =>
            new SalesManagement.Domain.Entities.BusinessUnit
            {
                Id = id,
                BusinessUnitCode = "BU001",
                BusinessUnitName = "Test Business Unit",
                Description = "Test Description",
                IsActive = SalesManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = SalesManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
