using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotationAmendment;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment
{
    public interface ISalesQuotationAmendmentCommandRepository
    {
        Task<int> CreateAsync(SalesQuotationAmendmentHeader entity, List<SalesQuotationAmendmentDetail> details);
        Task<bool> ApplyAmendmentAsync(int amendmentHeaderId, string status, int modifiedBy, string? modifiedByName, string? modifiedIP, CancellationToken ct);
        Task<SalesQuotationAmendmentHeader?> GetByIdEntityAsync(int id);
        Task<SalesQuotationHeader?> GetSalesQuotationEntityAsync(int salesQuotationHeaderId);
        Task<AmendmentWorkFlowDto> GetByIdAmendmentWorkFlowAsync(int id);
    }
}
