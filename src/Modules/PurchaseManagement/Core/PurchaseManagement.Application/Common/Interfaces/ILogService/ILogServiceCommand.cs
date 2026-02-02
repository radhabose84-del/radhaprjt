using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.Application.Common.Interfaces.ILogService
{
    public interface ILogServiceCommand
    {
        Task<bool> CreateAsync(IndentLog indentLog);  
    }
}