using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.ILogService;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.LogServices
{
    public class LogServiceCommandRepository : ILogServiceCommand
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IIPAddressService _ipAddressService;
        public LogServiceCommandRepository(ApplicationDbContext dbContext, IIPAddressService ipAddressService)
        {
            _dbContext = dbContext;
            _ipAddressService = ipAddressService;
        }
        public async Task<bool> CreateAsync(IndentLog indentLog)
        {
            indentLog.CreatedBy = _ipAddressService.GetUserId();
            indentLog.CreatedByName = _ipAddressService.GetUserName();
            indentLog.CreatedDate = DateTime.Now;
            indentLog.CreatedIP = _ipAddressService.GetSystemIPAddress();

             _dbContext.Entry(indentLog);
            await _dbContext.IndentLog.AddAsync(indentLog);
            await _dbContext.SaveChangesAsync();

            return indentLog.Id > 0;
        }
    }
}