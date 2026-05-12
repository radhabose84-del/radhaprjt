using SalesManagement.Application.SalesAgreement.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesAgreement
{
    public interface ISalesAgreementQueryRepository
    {
        Task<(List<SalesAgreementHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesAgreementHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<SalesAgreementLookupDto>> AutocompleteAsync(string term, CancellationToken ct);

        Task<bool> NotFoundAsync(int id);
        Task<bool> SalesGroupExistsAsync(int salesGroupId);
        Task<bool> StatusExistsAsync(int statusId);
        Task<bool> CustomerExistsAsync(int customerId);
        Task<bool> PaymentTermsExistsAsync(int paymentTermsId);
        Task<bool> ItemExistsAsync(int itemId);
        Task<bool> VariantExistsAsync(int variantId);
    }
}
