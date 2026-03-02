using SalesManagement.Application.SalesLead.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesLead
{
    public interface ISalesLeadQueryRepository
    {
        Task<(List<SalesLeadDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesLeadDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<SalesLeadLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> MobileNumberExistsForProspectAsync(string mobileNumber, int? excludeId = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ContactExistsAsync(int contactId);
        Task<bool> LeadSourceExistsAsync(int leadSourceId);
        Task<bool> PartyExistsAsync(int partyId);
        Task<bool> CityExistsAsync(int cityId);
        Task<bool> ItemExistsAsync(int itemId);
        Task<bool> MarketingOfficerExistsAsync(int marketingOfficerId);
        Task<bool> MobileNumberExistsInSalesContactAsync(string mobileNumber);
        Task<int> GetPrimaryContactTypeIdAsync();
    }
}
