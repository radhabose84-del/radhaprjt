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

        public static AgentCommissionSlabItem ValidSlab() =>
            new()
            {
                SlabOrder = 1,
                FromDelay = 0,
                ToDelay = 30,
                CommissionTypeId = 40,
                CommissionBasisId = 70,
                CommissionValue = 5.0m
            };

        public static CreateAgentCommissionConfigCommand ValidCreateCommand(
            int agentId = 10,
            int commissionTypeId = 40,
            int commissionBasisId = 70,
            int applicableLevelId = 80,
            int triggerEventId = 90,
            int? slabTypeId = 100,
            int commissionSplitId = 110,
            decimal commissionPercentage = 5.0m,
            DateTimeOffset? validityFrom = null,
            DateTimeOffset? validityTo = null,
            List<int>? salesGroupIds = null,
            List<int>? paymentTermIds = null,
            List<AgentCommissionSlabItem>? slabs = null)
            => new()
            {
                AgentId = agentId,
                CommissionTypeId = commissionTypeId,
                CommissionBasisId = commissionBasisId,
                ApplicableLevelId = applicableLevelId,
                TriggerEventId = triggerEventId,
                SlabTypeId = slabTypeId,
                CommissionSplitId = commissionSplitId,
                CommissionPercentage = commissionPercentage,
                ValidityFrom = validityFrom ?? DefaultValidityFrom,
                ValidityTo = validityTo ?? DefaultValidityTo,
                SalesGroupIds = salesGroupIds,
                PaymentTermIds = paymentTermIds,
                Slabs = slabs ?? new List<AgentCommissionSlabItem> { ValidSlab() }
            };

        public static UpdateAgentCommissionConfigCommand ValidUpdateCommand(
            int id = 1,
            int agentId = 10,
            int commissionTypeId = 40,
            int commissionBasisId = 70,
            int applicableLevelId = 80,
            int triggerEventId = 90,
            int? slabTypeId = 100,
            int commissionSplitId = 110,
            decimal commissionPercentage = 5.0m,
            DateTimeOffset? validityFrom = null,
            DateTimeOffset? validityTo = null,
            int isActive = 1,
            List<int>? salesGroupIds = null,
            List<int>? paymentTermIds = null,
            List<AgentCommissionSlabItem>? slabs = null)
            => new()
            {
                Id = id,
                AgentId = agentId,
                CommissionTypeId = commissionTypeId,
                CommissionBasisId = commissionBasisId,
                ApplicableLevelId = applicableLevelId,
                TriggerEventId = triggerEventId,
                SlabTypeId = slabTypeId,
                CommissionSplitId = commissionSplitId,
                CommissionPercentage = commissionPercentage,
                ValidityFrom = validityFrom ?? DefaultValidityFrom,
                ValidityTo = validityTo ?? DefaultValidityTo,
                IsActive = isActive,
                SalesGroupIds = salesGroupIds,
                PaymentTermIds = paymentTermIds,
                Slabs = slabs ?? new List<AgentCommissionSlabItem> { ValidSlab() }
            };

        public static AgentCommissionConfigDto ValidDto(
            int id = 1,
            int agentId = 10,
            string? agentName = "Test Agent",
            int commissionTypeId = 40,
            string? commissionTypeName = "Percentage",
            int commissionBasisId = 70,
            string? commissionBasisName = "Invoice Value",
            int applicableLevelId = 80,
            string? applicableLevelName = "Header",
            decimal commissionPercentage = 5.0m,
            int commissionSplitId = 110,
            string? splitCode = "SPL01",
            string? splitName = "Default Split")
            => new()
            {
                Id = id,
                AgentId = agentId,
                AgentName = agentName,
                CommissionTypeId = commissionTypeId,
                CommissionTypeName = commissionTypeName,
                CommissionBasisId = commissionBasisId,
                CommissionBasisName = commissionBasisName,
                ApplicableLevelId = applicableLevelId,
                ApplicableLevelName = applicableLevelName,
                CommissionPercentage = commissionPercentage,
                CommissionSplitId = commissionSplitId,
                SplitCode = splitCode,
                SplitName = splitName,
                ValidityFrom = DefaultValidityFrom,
                ValidityTo = DefaultValidityTo,
                IsActive = true,
                IsDeleted = false
            };

        public static IReadOnlyList<AgentCommissionConfigLookupDto> ValidLookupList()
            => new List<AgentCommissionConfigLookupDto>
            {
                new() { Id = 1, AgentName = "Agent A", SplitCode = "SPL01" },
                new() { Id = 2, AgentName = "Agent B", SplitCode = "SPL02" }
            };

        public static DomainEntities.AgentCommissionConfig ValidEntity(
            int id = 1,
            int agentId = 10,
            int commissionTypeId = 40)
            => new()
            {
                Id = id,
                AgentId = agentId,
                CommissionTypeId = commissionTypeId,
                CommissionBasisId = 70,
                ApplicableLevelId = 80,
                TriggerEventId = 90,
                SlabTypeId = 100,
                CommissionSplitId = 110,
                CommissionPercentage = 5.0m,
                ValidityFrom = DefaultValidityFrom,
                ValidityTo = DefaultValidityTo,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
