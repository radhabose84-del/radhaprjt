using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartMasterAutoComplete;
using PartyManagement.Application.PartyMaster.Queries.GetPartyGroupLoad;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterPending;
using PartyManagement.Domain.Entities;

namespace PartyManagement.Application.Common.Interfaces.IPartyMaster
{
    public interface IPartyMasterQueryRepository
    {
        Task<List<PartyGroupLoadDto>> GetPartyGroupsAsync(List<int> groupTypeIds);
        Task<string> GetDocumentDirectoryAsync();
        Task<string> GetBaseDirectoryAsync();
        Task<PartyMasterDto> GetByIdPartyMasterAsync(int id);
        Task<(List<GetPartyMasterDto>, int)> GetAllPartyMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<GetPartyMasterAutoCompleteDto>> GetPartyMasterAutoComplete(List<int> partyTypeIds, string searchPattern);
        Task<List<GetPartyMasterAutoCompleteDto>> GetPartyMasterAutoComplete(string searchPattern);
        Task<(List<PartyMasterPendingDto>, int)> GetAllPartyMasterPendingAsync(string? SearchTerm);

        Task<(IReadOnlyList<int> CompanyIds, IReadOnlyList<int> UnitIds)> GetCompanyUnitMapAsync(int partyId);

        Task<IReadOnlyList<string>> GetPartyTypeCodesAsync(int partyId);
        Task<RegistrationDto> GetRegistrationDetails(int RegistrationTypeId);
        Task<Dictionary<string, string>> GetDocumentDirectoryPath();
      
    }
}