using PurchaseManagement.Application.PartyMaster.Queries.GetPartyDetails;
// using PurchaseManagement.Application.PartyMaster.Queries.GetPartyDetails;

namespace PurchaseManagement.Application.Common.Interfaces.IPartyMaster
{
    public interface IPartyMasterQueryRepository
    {
        Task<List<PartyMasterDTO>> GetPartyMasters(string OldunitCode, string searchPattern);
        
         
    }
}