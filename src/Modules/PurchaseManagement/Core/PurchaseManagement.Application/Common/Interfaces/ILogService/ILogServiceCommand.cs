using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.Application.Common.Interfaces.ILogService
{
    public interface ILogServiceCommand
    {
        Task<bool> CreateAsync(IndentLog indentLog);  
    }
}