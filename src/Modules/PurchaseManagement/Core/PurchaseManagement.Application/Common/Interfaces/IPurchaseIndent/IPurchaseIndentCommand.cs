using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent
{
    public interface IPurchaseIndentCommand
    {
        Task<IndentHeader> CreateAsync(IndentHeader indentHeader);
        Task<bool> UpdateAsync(IndentHeader indentHeader, string request);
        Task<bool> DeleteAsync(int id, IndentHeader indentHeader);
        // Task<List<IndentDetail>> UpdateIndentDetailAsync(List<IndentDetail> indentDetail);
        Task<bool> RollbackStatusAsync(int id);
        Task<bool> FinalizeStatus(IndentHeader indentHeader);
        Task<bool> UpdateRFQStatusAsync(IEnumerable<int> indentDetailIds);
    }
}