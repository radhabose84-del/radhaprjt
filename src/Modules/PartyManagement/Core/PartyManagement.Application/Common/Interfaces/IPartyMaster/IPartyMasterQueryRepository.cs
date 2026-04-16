using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartMasterAutoComplete;
using PartyManagement.Application.PartyMaster.Queries.GetPartyGroupLoad;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterPending;

namespace PartyManagement.Application.Common.Interfaces.IPartyMaster
{
    public interface IPartyMasterQueryRepository
    {
        Task<List<PartyGroupLoadDto>> GetPartyGroupsAsync(List<int> groupTypeIds);
        Task<string> GetDocumentDirectoryAsync();
        Task<string> GetBaseDirectoryAsync();
        Task<PartyMasterDto> GetByIdPartyMasterAsync(int id);
        Task<(List<GetPartyMasterDto>, int)> GetAllPartyMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<GetPartyMasterAutoCompleteDto>> GetPartyMasterAutoComplete(List<int> partyTypeIds, string searchPattern, int? agentId = null);
        Task<List<GetPartyMasterAutoCompleteDto>> GetPartyMasterAutoComplete(string searchPattern);
        Task<(List<PartyMasterPendingDto>, int)> GetAllPartyMasterPendingAsync(string? SearchTerm);

        Task<(IReadOnlyList<int> CompanyIds, IReadOnlyList<int> UnitIds)> GetCompanyUnitMapAsync(int partyId);

        Task<IReadOnlyList<string>> GetPartyTypeCodesAsync(int partyId);
        Task<RegistrationDto> GetRegistrationDetails(int RegistrationTypeId);
        Task<Dictionary<string, string>> GetDocumentDirectoryPath();
        Task<bool> TransportDetailDuplicateExistsAsync(int? defaultFreightTypeId, int? vehicleTypeId, string vehicleNo, int? excludeId = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsPartyMasterLinkedAsync(int id);
    }
}