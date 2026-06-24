namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalFlag
{
    // Active threshold rule projected for the flagging engine (US-GL01-16B).
    public sealed class ActiveThresholdRule
    {
        public int RuleTypeId { get; set; }
        public string? RuleTypeCode { get; set; }
        public decimal? ThresholdValue { get; set; }
    }

    public interface IJournalFlagEngineRepository
    {
        // 16A rules that are active (Active = 1, not deleted), with their MiscMaster rule-type code.
        Task<IReadOnlyList<ActiveThresholdRule>> GetActiveThresholdRulesAsync(CancellationToken ct);

        // Persists raised flags (alert-only).
        Task AddFlagsAsync(IEnumerable<FinanceManagement.Domain.Entities.JournalFlag> flags, CancellationToken ct);

        // Daily digest support: flags not yet included in a digest, then mark them sent.
        Task<IReadOnlyList<FinanceManagement.Domain.Entities.JournalFlag>> GetUndigestedFlagsAsync(CancellationToken ct);
        Task MarkDigestSentAsync(IEnumerable<int> flagIds, CancellationToken ct);
    }
}
