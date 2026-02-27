using SalesManagement.Application.SalesQuotation.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesQuotation
{
    public interface ISalesQuotationQueryRepository
    {
        Task<(List<SalesQuotationHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesQuotationHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<SalesQuotationLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> CustomerExistsAsync(int customerId);
        Task<bool> PaymentTermExistsAsync(int paymentTermId);
        Task<bool> ItemExistsAsync(int itemId);
        Task<bool> HSNExistsAsync(int hsnId);
        Task<bool> DeliveryTermExistsAsync(int deliveryTermId);
        Task<bool> ContactPersonExistsAsync(int contactPersonId);
        Task<bool> SalesEnquiryExistsAsync(int salesEnquiryId);
    }
}
