using SalesManagement.Application.SalesOrganisation.Commands.CreateSalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Dto;

namespace SalesManagement.UnitTests.TestData
{
    /// <summary>
    /// Builder helpers for creating valid test data for SalesOrganisation tests.
    /// </summary>
    public static class SalesOrganisationBuilders
    {
        // ── Create Command ────────────────────────────────────────────────────

        public static CreateSalesOrganisationCommand ValidCreateCommand(
            string? code = "ORG001",
            string? name = "Test Sales Organisation",
            int companyId = 1,
            string? description = "Test Description") =>
            new CreateSalesOrganisationCommand
            {
                SalesOrganisationCode = code!,
                SalesOrganisationName = name!,
                CompanyId = companyId,
                Description = description!
            };

        // ── Update Command ────────────────────────────────────────────────────

        public static UpdateSalesOrganisationCommand ValidUpdateCommand(
            int id = 1,
            string? name = "Updated Sales Organisation",
            int companyId = 1,
            string? description = "Updated Description",
            int isActive = 1) =>
            new UpdateSalesOrganisationCommand
            {
                Id = id,
                SalesOrganisationName = name!,
                CompanyId = companyId,
                Description = description!,
                IsActive = isActive
            };

        // ── Full DTO ──────────────────────────────────────────────────────────

        public static SalesOrganisationDto ValidDto(
            int id = 1,
            string code = "ORG001",
            string name = "Test Sales Organisation",
            int companyId = 1,
            string companyName = "Test Company") =>
            new SalesOrganisationDto
            {
                Id = id,
                SalesOrganisationCode = code,
                SalesOrganisationName = name,
                CompanyId = companyId,
                CompanyName = companyName,
                Description = "Test Description",
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        // ── Lookup DTO List ───────────────────────────────────────────────────

        public static IReadOnlyList<SalesOrganisationLookupDto> ValidLookupList() =>
            new List<SalesOrganisationLookupDto>
            {
                new SalesOrganisationLookupDto { Id = 1, SalesOrganisationCode = "ORG001", SalesOrganisationName = "Org One" },
                new SalesOrganisationLookupDto { Id = 2, SalesOrganisationCode = "ORG002", SalesOrganisationName = "Org Two" }
            };

        // ── Domain Entity ─────────────────────────────────────────────────────

        public static SalesManagement.Domain.Entities.SalesOrganisation ValidEntity(int id = 1) =>
            new SalesManagement.Domain.Entities.SalesOrganisation
            {
                Id = id,
                SalesOrganisationCode = "ORG001",
                SalesOrganisationName = "Test Sales Organisation",
                CompanyId = 1,
                Description = "Test",
                IsActive = SalesManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = SalesManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
