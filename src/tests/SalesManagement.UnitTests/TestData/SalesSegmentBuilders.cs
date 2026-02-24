#nullable disable
using Contracts.Dtos.Lookups.Users;
using SalesManagement.Application.SalesSegment.Commands.CreateSalesSegment;
using SalesManagement.Application.SalesSegment.Commands.UpdateSalesSegment;
using SalesManagement.Application.SalesSegment.Dto;

namespace SalesManagement.UnitTests.TestData
{
    /// <summary>
    /// Builder helpers for creating valid test data for SalesSegment tests.
    /// SalesSegment has NO code field — it is identified by a composite key
    /// (SalesOrganisationId + SalesChannelId + BusinessUnitId).
    /// CurrencyId is optional and validated via ICurrencyLookup (cross-module).
    /// </summary>
    public static class SalesSegmentBuilders
    {
        // ── Create Command ────────────────────────────────────────────────────

        public static CreateSalesSegmentCommand ValidCreateCommand(
            int salesOrganisationId = 1,
            int salesChannelId = 1,
            int businessUnitId = 1,
            int? currencyId = null,
            string segmentName = "Test Segment",
            DateTime? validFrom = null) =>
            new CreateSalesSegmentCommand
            {
                SalesOrganisationId = salesOrganisationId,
                SalesChannelId = salesChannelId,
                BusinessUnitId = businessUnitId,
                CurrencyId = currencyId,
                SegmentName = segmentName,
                ValidFrom = validFrom
            };

        // ── Update Command ────────────────────────────────────────────────────

        public static UpdateSalesSegmentCommand ValidUpdateCommand(
            int id = 1,
            string segmentName = "Updated Segment",
            int? currencyId = null,
            int isActive = 1,
            DateTime? validFrom = null) =>
            new UpdateSalesSegmentCommand
            {
                Id = id,
                SegmentName = segmentName,
                CurrencyId = currencyId,
                IsActive = isActive,
                ValidFrom = validFrom
            };

        // ── Full DTO ──────────────────────────────────────────────────────────

        public static SalesSegmentDto ValidDto(
            int id = 1,
            int salesOrganisationId = 1,
            int salesChannelId = 1,
            int businessUnitId = 1,
            string segmentName = "Test Segment",
            int? currencyId = null,
            string currencyName = null) =>
            new SalesSegmentDto
            {
                Id = id,
                SalesOrganisationId = salesOrganisationId,
                SalesOrganisationName = "Test Sales Org",
                SalesChannelId = salesChannelId,
                SalesChannelName = "Test Sales Channel",
                BusinessUnitId = businessUnitId,
                BusinessUnitName = "Test Business Unit",
                CurrencyId = currencyId,
                CurrencyName = currencyName,
                SegmentName = segmentName,
                ValidFrom = null,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user"
            };

        // ── Lookup DTO List ───────────────────────────────────────────────────

        public static IReadOnlyList<SalesSegmentLookupDto> ValidLookupList() =>
            new List<SalesSegmentLookupDto>
            {
                new SalesSegmentLookupDto { Id = 1, SegmentName = "Segment One", SalesOrganisationId = 1, SalesChannelId = 1, BusinessUnitId = 1 },
                new SalesSegmentLookupDto { Id = 2, SegmentName = "Segment Two", SalesOrganisationId = 1, SalesChannelId = 1, BusinessUnitId = 2 }
            };

        // ── Currency Lookup ───────────────────────────────────────────────────

        public static IReadOnlyList<CurrencyLookupDto> ValidCurrencyList(int currencyId = 5) =>
            new List<CurrencyLookupDto>
            {
                new CurrencyLookupDto { CurrencyId = currencyId, Code = "USD", Name = "US Dollar" }
            };

        public static IReadOnlyList<CurrencyLookupDto> EmptyCurrencyList() =>
            new List<CurrencyLookupDto>();

        // ── Domain Entity ─────────────────────────────────────────────────────

        public static SalesManagement.Domain.Entities.SalesSegment ValidEntity(int id = 1) =>
            new SalesManagement.Domain.Entities.SalesSegment
            {
                Id = id,
                SalesOrganisationId = 1,
                SalesChannelId = 1,
                BusinessUnitId = 1,
                SegmentName = "Test Segment",
                IsActive = SalesManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = SalesManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
