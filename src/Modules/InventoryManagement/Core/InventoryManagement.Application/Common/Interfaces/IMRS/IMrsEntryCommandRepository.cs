using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Domain.Entities.MRS;

namespace InventoryManagement.Application.Common.Interfaces.IMRS
{
    public interface IMrsEntryCommandRepository
    {
        Task<string> GenerateNextCodeAsync(CancellationToken ct = default);
        Task<MrsHeader> CreateAsync(MrsHeader mrsHeader);
        Task<bool> UpdateAsync(MrsHeader mrsHeader);
        Task<bool> UpdateMrsApproveAsync(int id, int statusId, CancellationToken ct = default);    
    }
}