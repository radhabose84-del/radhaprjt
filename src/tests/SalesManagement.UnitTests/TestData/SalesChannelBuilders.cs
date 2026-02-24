#nullable disable
using SalesManagement.Application.SalesChannel.Commands.CreateSalesChannel;
using SalesManagement.Application.SalesChannel.Commands.UpdateSalesChannel;
using SalesManagement.Application.SalesChannel.Dto;

namespace SalesManagement.UnitTests.TestData
{
    /// <summary>
    /// Builder helpers for creating valid test data for SalesChannel tests.
    /// SalesChannel is self-contained — no CompanyId or Description fields.
    /// </summary>
    public static class SalesChannelBuilders
    {
        // ── Create Command ────────────────────────────────────────────────────

        public static CreateSalesChannelCommand ValidCreateCommand(
            string code = "CH001",
            string name = "Test Sales Channel") =>
            new CreateSalesChannelCommand
            {
                SalesChannelCode = code,
                SalesChannelName = name
            };

        // ── Update Command ────────────────────────────────────────────────────

        public static UpdateSalesChannelCommand ValidUpdateCommand(
            int id = 1,
            string name = "Updated Sales Channel",
            int isActive = 1) =>
            new UpdateSalesChannelCommand
            {
                Id = id,
                SalesChannelName = name,
                IsActive = isActive
            };

        // ── Full DTO ──────────────────────────────────────────────────────────

        public static SalesChannelDto ValidDto(
            int id = 1,
            string code = "CH001",
            string name = "Test Sales Channel") =>
            new SalesChannelDto
            {
                Id = id,
                SalesChannelCode = code,
                SalesChannelName = name,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

        // ── Lookup DTO List ───────────────────────────────────────────────────

        public static IReadOnlyList<SalesChannelLookupDto> ValidLookupList() =>
            new List<SalesChannelLookupDto>
            {
                new SalesChannelLookupDto { Id = 1, SalesChannelCode = "CH001", SalesChannelName = "Channel One" },
                new SalesChannelLookupDto { Id = 2, SalesChannelCode = "CH002", SalesChannelName = "Channel Two" }
            };

        // ── Domain Entity ─────────────────────────────────────────────────────

        public static SalesManagement.Domain.Entities.SalesChannel ValidEntity(int id = 1) =>
            new SalesManagement.Domain.Entities.SalesChannel
            {
                Id = id,
                SalesChannelCode = "CH001",
                SalesChannelName = "Test Sales Channel",
                IsActive = SalesManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = SalesManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
