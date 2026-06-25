using Contracts.Common;

namespace FinanceManagement.Application.Common.PeriodStatus
{
    /// <summary>
    /// US-GL03-02 — enforces the one-way period-status flow.
    /// Forward transitions are validated here; reverse transitions
    /// route through PeriodStatusOverride (CFO + SysAdmin dual approval).
    /// </summary>
    public static class PeriodStatusStateMachine
    {
        /// <summary>
        /// Throws ExceptionRules if (fromCode -> toCode) is not a legal direct forward transition.
        /// Legal: OPEN -> SOFTCLOSED, SOFTCLOSED -> HARDCLOSED.
        /// </summary>
        public static void AssertForwardTransition(string? fromCode, string toCode)
        {
            if (string.IsNullOrWhiteSpace(fromCode))
                throw new ExceptionRules("Current period status is unknown.");

            var from = fromCode.ToUpperInvariant();
            var to   = toCode.ToUpperInvariant();

            var legal =
                (from == PeriodStatusConstants.Open       && to == PeriodStatusConstants.SoftClosed) ||
                (from == PeriodStatusConstants.SoftClosed && to == PeriodStatusConstants.HardClosed);

            if (!legal)
                throw new ExceptionRules(
                    $"Illegal status transition: {from} → {to}. " +
                    $"Allowed: OPEN → SOFTCLOSED, SOFTCLOSED → HARDCLOSED.");
        }

        /// <summary>
        /// True only for legal reversal pairs (require CFO + SysAdmin override).
        /// Legal: HARDCLOSED -> SOFTCLOSED, SOFTCLOSED -> OPEN.
        /// </summary>
        public static bool IsValidReversal(string? fromCode, string? toCode)
        {
            if (string.IsNullOrWhiteSpace(fromCode) || string.IsNullOrWhiteSpace(toCode))
                return false;

            var from = fromCode.ToUpperInvariant();
            var to   = toCode.ToUpperInvariant();

            return
                (from == PeriodStatusConstants.HardClosed && to == PeriodStatusConstants.SoftClosed) ||
                (from == PeriodStatusConstants.SoftClosed && to == PeriodStatusConstants.Open);
        }
    }
}
