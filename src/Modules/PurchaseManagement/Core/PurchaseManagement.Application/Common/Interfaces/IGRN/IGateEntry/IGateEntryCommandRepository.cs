using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;

namespace PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry
{
    public interface IGateEntryCommandRepository
    {
        Task<int> CreateAsync(GateEntryHeader gateEntryHeader);
        Task<bool> UpdateAsync(int Id, GateEntryHeader gateEntryHeader);
        Task<bool> DeleteAsync(int Id, GateEntryHeader gateEntryHeader);
        Task<string> GenerateNextCodeAsync(CancellationToken ct = default);
    }
}