using PartyManagement.Application.PartyGroup.Queries.GetPartyGroup;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupAutoComplete;
using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupById;

namespace PartyManagement.Application.Common.Interfaces.IPartyGroup
{
    public interface IPartyGroupQueryRepository
    {
        Task<PartyGroupByIdDto?> GetByIdAsync(int Id);
        Task<(List<PartyGroupDto>, int)> GetAllPartyGroupAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<PartyGroupAutoCompleteDto>> GetMainPartyGroups(string searchPattern);
        Task<List<PartyGroupAutoCompleteDto>> GetParentPartyGroups(string searchPattern);
     
    }
}