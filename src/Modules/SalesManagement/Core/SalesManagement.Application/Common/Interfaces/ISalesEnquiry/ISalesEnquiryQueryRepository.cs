using SalesManagement.Application.SalesEnquiry.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesEnquiry
{
    public interface ISalesEnquiryQueryRepository
    {
        Task<(List<SalesEnquiryHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesEnquiryHeaderDto?> GetByIdAsync(int id);
        Task<bool> NotFoundAsync(int id);
        Task<bool> PartyExistsAsync(int partyId);
        Task<bool> PaymentTermExistsAsync(int paymentTermId);
        Task<bool> ItemExistsAsync(int itemId);
    }
}
