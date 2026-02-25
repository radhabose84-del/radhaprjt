#nullable disable
using SalesManagement.Application.SalesOffice.Commands.CreateSalesOffice;
using SalesManagement.Application.SalesOffice.Commands.UpdateSalesOffice;
using SalesManagement.Application.SalesOffice.Dto;

namespace SalesManagement.UnitTests.TestData
{
    /// <summary>
    /// Builder helpers for creating valid test data for SalesOffice tests.
    /// SalesOffice has FKs to SalesOrganisation (same-module) and City (cross-module).
    /// </summary>
    public static class SalesOfficeBuilders
    {
        // ── Create Command ────────────────────────────────────────────────────

        public static CreateSalesOfficeCommand ValidCreateCommand(
            string name = "Test Sales Office",
            int salesOrganisationId = 1,
            int cityId = 1,
            string pincode = "560001",
            string phone = "+911234567890",
            string email = "office@test.com",
            string responsibleManager = "Jane Doe",
            string regionTerritory = "South",
            string address = "123 Test Street") =>
            new CreateSalesOfficeCommand
            {
                SalesOfficeName = name,
                SalesOrganisationId = salesOrganisationId,
                CityId = cityId,
                Pincode = pincode,
                Phone = phone,
                Email = email,
                ResponsibleManager = responsibleManager,
                RegionTerritory = regionTerritory,
                Address = address
            };

        // ── Update Command ────────────────────────────────────────────────────

        public static UpdateSalesOfficeCommand ValidUpdateCommand(
            int id = 1,
            string name = "Updated Sales Office",
            int salesOrganisationId = 1,
            int cityId = 1,
            string pincode = "560002",
            string phone = "+911234567891",
            string email = "updated@test.com",
            string responsibleManager = "John Doe",
            string regionTerritory = "North",
            string address = "456 Updated Street",
            int isActive = 1) =>
            new UpdateSalesOfficeCommand
            {
                Id = id,
                SalesOfficeName = name,
                SalesOrganisationId = salesOrganisationId,
                CityId = cityId,
                Pincode = pincode,
                Phone = phone,
                Email = email,
                ResponsibleManager = responsibleManager,
                RegionTerritory = regionTerritory,
                Address = address,
                IsActive = isActive
            };

        // ── Full DTO ──────────────────────────────────────────────────────────

        public static SalesOfficeDto ValidDto(
            int id = 1,
            string name = "Test Sales Office",
            int salesOrganisationId = 1,
            string salesOrganisationName = "Test Sales Org",
            int cityId = 1,
            string cityName = "Bangalore") =>
            new SalesOfficeDto
            {
                Id = id,
                SalesOfficeName = name,
                SalesOrganisationId = salesOrganisationId,
                SalesOrganisationName = salesOrganisationName,
                CityId = cityId,
                CityName = cityName,
                Pincode = "560001",
                Phone = "+911234567890",
                Email = "office@test.com",
                ResponsibleManager = "Jane Doe",
                RegionTerritory = "South",
                Address = "123 Test Street",
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        // ── Lookup DTO List ───────────────────────────────────────────────────

        public static IReadOnlyList<SalesOfficeLookupDto> ValidLookupList() =>
            new List<SalesOfficeLookupDto>
            {
                new SalesOfficeLookupDto { Id = 1, SalesOfficeName = "Office One", SalesOrganisationId = 1 },
                new SalesOfficeLookupDto { Id = 2, SalesOfficeName = "Office Two", SalesOrganisationId = 1 }
            };

        // ── Domain Entity ─────────────────────────────────────────────────────

        public static SalesManagement.Domain.Entities.SalesOffice ValidEntity(int id = 1) =>
            new SalesManagement.Domain.Entities.SalesOffice
            {
                Id = id,
                SalesOfficeName = "Test Sales Office",
                SalesOrganisationId = 1,
                CityId = 1,
                Pincode = "560001",
                Phone = "+911234567890",
                Email = "office@test.com",
                ResponsibleManager = "Jane Doe",
                RegionTerritory = "South",
                Address = "123 Test Street",
                IsActive = SalesManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = SalesManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
