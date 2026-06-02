namespace Contracts.Dtos.Budget
{
    /// <summary>
    /// Approved vs remaining allocation for a budget group/period — used to render the
    /// Budget panel ("% utilised" + Positive/Negative) on a Purchase Order.
    /// </summary>
    public class BudgetAllocationSummaryDto
    {
        public decimal ApprovedAmount { get; set; }
        public decimal RemainingBalance { get; set; }
    }
}
