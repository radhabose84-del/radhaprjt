using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent
{
    public interface IPurchaseIndentGrpcQuery
    {
        Task<IndentHeader> GetByIdGrpcAsync(int id);
    }
}