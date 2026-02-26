using SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Commands.UpdateAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Dto;
using static SalesManagement.Domain.Common.BaseEntity;
using DomainEntities = SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.TestData
{
    public static class AgentCommissionConfigBuilders
    {
        private static readonly DateTimeOffset DefaultValidityFrom = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset DefaultValidityTo = new(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);

        public static CreateAgentCommissionConfigCommand ValidCreateCommand(
            int agentId = 10,
            int salesSegmentId = 20,
            int itemId = 30,
            int commissionTypeId = 40,
            int? uomId = 50,
            decimal commissionPercentage = 5.0m,
            int? currencyId = 60,
            decimal? subAgentPercentage = 2.0m,
            DateTimeOffset? validityFrom = null,
            DateTimeOffset? validityTo = null)
            => new()
            {
                AgentId = agentId,
                SalesSegmentId = salesSegmentId,
                ItemId = itemId,
                CommissionTypeId = commissionTypeId,
                UomId = uomId,
                CommissionPercentage = commissionPercentage,
                CurrencyId = currencyId,
                SubAgentPercentage = subAgentPercentage,
                ValidityFrom = validityFrom ?? DefaultValidityFrom,
                ValidityTo = validityTo ?? DefaultValidityTo
            };

        public static UpdateAgentCommissionConfigCommand ValidUpdateCommand(
            int id = 1,
            int agentId = 10,
            int salesSegmentId = 20,
            int itemId = 30,
            int commissionTypeId = 40,
            int? uomId = 50,
            decimal commissionPercentage = 5.0m,
            int? currencyId = 60,
            decimal? subAgentPercentage = 2.0m,
            DateTimeOffset? validityFrom = null,
            DateTimeOffset? validityTo = null,
            int isActive = 1)
            => new()
            {
                Id = id,
                AgentId = agentId,
                SalesSegmentId = salesSegmentId,
                ItemId = itemId,
                CommissionTypeId = commissionTypeId,
                UomId = uomId,
                CommissionPercentage = commissionPercentage,
                CurrencyId = currencyId,
                SubAgentPercentage = subAgentPercentage,
                ValidityFrom = validityFrom ?? DefaultValidityFrom,
                ValidityTo = validityTo ?? DefaultValidityTo,
                IsActive = isActive
            };

        public static AgentCommissionConfigDto ValidDto(
            int id = 1,
            int agentId = 10,
            string? agentName = "Test Agent",
            int salesSegmentId = 20,
            string? segmentName = "Test Segment",
            int itemId = 30,
            string? itemName = "Test Item",
            int commissionTypeId = 40,
            string? commissionTypeName = "Flat Rate",
            int? uomId = 50,
            string? uomName = "KG",
            decimal commissionPercentage = 5.0m,
            int? currencyId = 60,
            string? currencyCode = "USD",
            decimal? subAgentPercentage = 2.0m)
            => new()
            {
                Id = id,
                AgentId = agentId,
                AgentName = agentName,
                SalesSegmentId = salesSegmentId,
                SegmentName = segmentName,
                ItemId = itemId,
                ItemName = itemName,
                CommissionTypeId = commissionTypeId,
                CommissionTypeName = commissionTypeName,
                UomId = uomId,
                UomName = uomName,
                CommissionPercentage = commissionPercentage,
                CurrencyId = currencyId,
                CurrencyCode = currencyCode,
                SubAgentPercentage = subAgentPercentage,
                ValidityFrom = DefaultValidityFrom,
                ValidityTo = DefaultValidityTo,
                IsActive = true,
                IsDeleted = false
            };

        public static IReadOnlyList<AgentCommissionConfigLookupDto> ValidLookupList()
            => new List<AgentCommissionConfigLookupDto>
            {
                new() { Id = 1, AgentName = "Agent A", ItemName = "Item A", SegmentName = "Segment A" },
                new() { Id = 2, AgentName = "Agent B", ItemName = "Item B", SegmentName = "Segment B" }
            };

        public static DomainEntities.AgentCommissionConfig ValidEntity(
            int id = 1,
            int agentId = 10,
            int salesSegmentId = 20,
            int itemId = 30,
            int commissionTypeId = 40)
            => new()
            {
                Id = id,
                AgentId = agentId,
                SalesSegmentId = salesSegmentId,
                ItemId = itemId,
                CommissionTypeId = commissionTypeId,
                UomId = 50,
                CommissionPercentage = 5.0m,
                CurrencyId = 60,
                SubAgentPercentage = 2.0m,
                ValidityFrom = DefaultValidityFrom,
                ValidityTo = DefaultValidityTo,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
