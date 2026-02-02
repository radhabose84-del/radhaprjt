using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.GRN.GateEntry.Queries.GetGateEntriesApprovedPo;

namespace PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry
{
    public interface IGateEntryQueryRepository
    {
        Task<string> GetDocumentDirectoryAsync();
        Task<List<GetGateEntriesApprovedPoDto>> GetGateEntriesApprovedPoDto(int partyId);
        

    }
}