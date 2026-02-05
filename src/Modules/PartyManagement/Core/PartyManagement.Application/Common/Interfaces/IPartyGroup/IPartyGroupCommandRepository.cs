using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyManagement.Application.Common.Interfaces.IPartyGroup
{
    public interface IPartyGroupCommandRepository
    {
        Task<int> CreateAsync(PartyManagement.Domain.Entities.PartyGroup partyGroup);
        Task<bool> UpdateAsync(int Id, PartyManagement.Domain.Entities.PartyGroup partyGroup);
        Task<bool> DeleteAsync(int Id, PartyManagement.Domain.Entities.PartyGroup partyGroup);
        Task<bool> ExistsAsync(string partyGroupName, int groupTypeId);
        Task<bool> ExistsUpdateAsync(string partyGroupName, int groupTypeId, int? currentId = null);
        Task<int?> GetGroupTypeIdByIdAsync(int id);
    }
}