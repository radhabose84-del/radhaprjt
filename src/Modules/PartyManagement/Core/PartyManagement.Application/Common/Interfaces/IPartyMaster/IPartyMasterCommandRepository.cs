using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
using PartyManagement.Domain.Entities;

namespace PartyManagement.Application.Common.Interfaces.IPartyMaster
{
    public interface IPartyMasterCommandRepository
    {
        Task<int> CreateAsync(PartyManagement.Domain.Entities.PartyMaster partyMaster);
        Task<bool> UpdateAsync(int Id, PartyManagement.Domain.Entities.PartyMaster partyMaster);
        Task<bool> DeleteAsync(int Id, PartyManagement.Domain.Entities.PartyMaster partyMaster);
        Task<string> GetNextPartyCodeAsync();
        Task<bool> DeleteFileDetailsDocumentAsync(int Id, int PartyId, string filename);
        Task<List<int>> GetPartyDocumentIdsAsync(int partyId);
        Task<bool> LogChange(int partyId, string tableName, string columnName, string oldValue, string newValue, string actionType);
        Task<bool> ExistsAsync(string partyname);
        Task<bool> ExistsForUpdateAsync(string partyName, int id);
        Task<bool> GstNumberExistsAsync(string gstNumber, int excludePartyId = 0);
        Task<PartyManagement.Domain.Entities.PartyMaster?> GetByIdAsync(int partyId);
        Task<bool> FinalizePartyStatus(PartyManagement.Domain.Entities.PartyMaster partyMaster);
        Task<bool> RollbackStatusAsync(int id);


        Task<PartyMasterWorkFlowDto> GetByIdPartyMasterWorkFlowAsync(int id);
        Task<PartyManagement.Domain.Entities.PartyMaster?> GetByIdWithContactsAsync(int partyId);
        Task<PartyContact?> GetPrimaryContactAsync(int partyId);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> MobileExistsAsync(string mobile);
        Task<bool> EmailExistsUpdateAsync(string email, int excludePartyId);
        Task<bool> MobileExistsUpdateAsync(string mobile, int excludePartyId);

        // NEW: get SUPPLIER/CUSTOMER/AGENT codes from PartyType → Misc (Code)
        Task<List<string>> GetPartyTypeCodesAsync(int partyId);
        

        

        
        
    }
}