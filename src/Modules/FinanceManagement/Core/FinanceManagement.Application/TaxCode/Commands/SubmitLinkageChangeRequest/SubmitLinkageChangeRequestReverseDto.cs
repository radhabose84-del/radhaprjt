using FinanceManagement.Application.TaxCode.Dto;

namespace FinanceManagement.Application.TaxCode.Commands.SubmitLinkageChangeRequest
{
    // Workflow payload wrapper (mirrors CreateSalesOrderReverseDto): the full linkage row is the
    // Header; linkages have no detail lines, so Lines is always null.
    public class SubmitLinkageChangeRequestReverseDto
    {
        public TaxAccountLinkageDto? Header { get; set; }
        public object? Lines { get; set; }
    }
}
