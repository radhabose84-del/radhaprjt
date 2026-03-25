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
            int commissionTypeId = 40,
            int? commissionBasisId = 70,
            int? applicableLevelId = 80,
            decimal commissionPercentage = 5.0m,
            int? currencyId = 60,
            DateTimeOffset? validityFrom = null,
            DateTimeOffset? validityTo = null)
            => new()
            {
                AgentId = agentId,
                SalesSegmentId = salesSegmentId,
                CommissionTypeId = commissionTypeId,
                CommissionBasisId = commissionBasisId,
                ApplicableLevelId = applicableLevelId,
                CommissionPercentage = commissionPercentage,
                CurrencyId = currencyId,
                ValidityFrom = validityFrom ?? DefaultValidityFrom,
                ValidityTo = validityTo ?? DefaultValidityTo
            };

        public static UpdateAgentCommissionConfigCommand ValidUpdateCommand(
            int id = 1,
            int agentId = 10,
            int salesSegmentId = 20,
            int commissionTypeId = 40,
            int? commissionBasisId = 70,
            int? applicableLevelId = 80,
            decimal commissionPercentage = 5.0m,
            int? currencyId = 60,
            DateTimeOffset? validityFrom = null,
            DateTimeOffset? validityTo = null,
            int isActive = 1)
            => new()
            {
                Id = id,
                AgentId = agentId,
                SalesSegmentId = salesSegmentId,
                CommissionTypeId = commissionTypeId,
                CommissionBasisId = commissionBasisId,
                ApplicableLevelId = applicableLevelId,
                CommissionPercentage = commissionPercentage,
                CurrencyId = currencyId,
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
            int commissionTypeId = 40,
            string? commissionTypeName = "Flat Rate",
            int? commissionBasisId = 70,
            string? commissionBasisName = "Per Unit",
            int? applicableLevelId = 80,
            string? applicableLevelName = "Order Level",
            decimal commissionPercentage = 5.0m,
            int? currencyId = 60,
            string? currencyCode = "USD")
            => new()
            {
                Id = id,
                AgentId = agentId,
                AgentName = agentName,
                SalesSegmentId = salesSegmentId,
                SegmentName = segmentName,
                CommissionTypeId = commissionTypeId,
                CommissionTypeName = commissionTypeName,
                CommissionBasisId = commissionBasisId,
                CommissionBasisName = commissionBasisName,
                ApplicableLevelId = applicableLevelId,
                ApplicableLevelName = applicableLevelName,
                CommissionPercentage = commissionPercentage,
                CurrencyId = currencyId,
                CurrencyCode = currencyCode,
                ValidityFrom = DefaultValidityFrom,
                ValidityTo = DefaultValidityTo,
                IsActive = true,
                IsDeleted = false
            };

        public static IReadOnlyList<AgentCommissionConfigLookupDto> ValidLookupList()
            => new List<AgentCommissionConfigLookupDto>
            {
                new() { Id = 1, AgentName = "Agent A", SegmentName = "Segment A" },
                new() { Id = 2, AgentName = "Agent B", SegmentName = "Segment B" }
            };

        public static DomainEntities.AgentCommissionConfig ValidEntity(
            int id = 1,
            int agentId = 10,
            int salesSegmentId = 20,
            int commissionTypeId = 40)
            => new()
            {
                Id = id,
                AgentId = agentId,
                SalesSegmentId = salesSegmentId,
                CommissionTypeId = commissionTypeId,
                CommissionBasisId = 70,
                ApplicableLevelId = 80,
                CommissionPercentage = 5.0m,
                CurrencyId = 60,
                ValidityFrom = DefaultValidityFrom,
                ValidityTo = DefaultValidityTo,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
